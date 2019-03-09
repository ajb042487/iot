﻿// Licensed to the .NET Foundation under one or more agreements.
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

        public void Write(byte[] payload)
        {
            Interop.Write(_handle, payload);
        }

        public void Listen(ICanRawListener listener, CancellationToken cancellationToken)
        {
            const int Size = 72;
            byte[] buffer = listener.GetBuffer(Size);
            if (buffer == null || buffer.Length < Size)
            {
                throw new ArgumentException($"GetBuffer did not provide buffer or the buffer was insufficiently small.");
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                var frame = new Interop.CanFrame();
                ReadCanFrame(ref frame);
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
