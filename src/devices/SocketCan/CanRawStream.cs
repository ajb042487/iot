// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace Iot.Device.SocketCan
{
    public class CanRawStream : IDisposable
    {
        private SafeCanRawSocketHandle _handle;
        public CanRawStream(string networkInterface = "can0")
        {
            _handle = new SafeCanRawSocketHandle(networkInterface);
        }

        // TODO: Remove me
        public void Write(byte[] payload)
        {
            Interop.Write(_handle, payload);
        }

        public unsafe void Listen(ICanRawListener listener, CancellationToken cancellationToken = default)
        {
            const int Size = 72;
            byte[] buffer = listener.GetBuffer(Size);

            if (buffer == null || buffer.Length < Size)
            {
                throw new ArgumentException($"GetBuffer did not provide buffer or the buffer was insufficiently small.");
            }

            fixed (byte* pinned = buffer)
            {
                Span<byte> buff = new Span<byte>(pinned, buffer.Length);
                Span<Interop.CanFrame> frameSpan = new Span<Interop.CanFrame>(pinned, 1);
                ref Interop.CanFrame frame = ref MemoryMarshal.GetReference(frameSpan);
                int dataOffset = (int)Marshal.OffsetOf(typeof(Interop.CanFrame), "data");

                while (!cancellationToken.IsCancellationRequested)
                {
                    ReadCanFrame(ref frame);
                    if (frame.can_dlc < 0 || frame.can_dlc > 16)
                    {
                        // we have a bad actor on the network
                        continue;
                    }

                    CanFlags flags = (CanFlags)(frame.can_id & (uint)(CanFlags.Error | CanFlags.ExtendedFrameFormat | CanFlags.RemoteTransmissionRequest));
                    
                    if (flags.HasFlag(CanFlags.Error) || (!flags.HasFlag(CanFlags.ExtendedFrameFormat) && IsEff(frame.can_id & Interop.CAN_EFF_MASK)))
                    {
                        // error flag or EFF address without EFF flag
                        continue;
                    }

                    listener.FrameReceived(frame.can_id, default, buff.Slice(dataOffset, frame.can_dlc));
                }
            }
        }

        private void ReadCanFrame(ref Interop.CanFrame frame)
        {
            Span<Interop.CanFrame> frameSpan = MemoryMarshal.CreateSpan(ref frame, 1);
            Span<byte> buff = MemoryMarshal.AsBytes(frameSpan);

            while (buff.Length > 0)
            {
                int read = Interop.Read(_handle, buff);
                buff = buff.Slice(read);
            }
        }

        // TODO: Remove me
        public void ReadTest()
        {
            const int MTU = 72;
            Span<byte> b = stackalloc byte[MTU];
            while (true)
            {
                int read = Interop.Read(_handle, b);
                Console.WriteLine(string.Join("", b.Slice(0, read).ToArray().Select((x) => x.ToString("X2"))));
            }
        }

        public void Filter(uint address)
        {
            Span<Interop.CanFilter> filters = stackalloc Interop.CanFilter[1];
            if (IsEff(address))
            {
                filters[0].can_id = (address & Interop.CAN_EFF_MASK) | (uint)CanFlags.ExtendedFrameFormat;
                filters[0].can_mask = Interop.CAN_EFF_MASK | (uint)CanFlags.ExtendedFrameFormat | (uint)CanFlags.RemoteTransmissionRequest;
            }
            else
            {
                filters[0].can_id = address & Interop.CAN_SFF_MASK;
                filters[0].can_mask = Interop.CAN_SFF_MASK | (uint)CanFlags.ExtendedFrameFormat | (uint)CanFlags.RemoteTransmissionRequest;
            }

            Interop.SetCanRawSocketOption<Interop.CanFilter>(_handle, Interop.CanSocketOption.CAN_RAW_FILTER, filters);
        }

        private bool _usingCanFd = false;
        public bool TrySwitchToCanFd()
        {
            if (_usingCanFd)
                throw new InvalidOperationException();
        }

        private static bool IsEff(uint address)
        {
            // has explicit flag or address does not fit in SFF addressing mode
            return (address & (uint)CanFlags.ExtendedFrameFormat) != 0
                || (address & Interop.CAN_EFF_MASK) != (address & Interop.CAN_SFF_MASK);
        }

        public void Dispose()
        {
            _handle.Dispose();
        }

        // private static Socket CreateSocket(string networkInterface)
        // {
        //     Type socketT = typeof(Socket);
        //     BindingFlags bfAll = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
        //     Socket ret = (Socket)FormatterServices.GetUninitializedObject(socketT);

        //     socketT.GetField("_handle", bfAll).SetValue(ret, new SafeSocketHandle(Interop.CreateCanRawSocket(networkInterface)));

        //     return ret;
        // }
    }
}
