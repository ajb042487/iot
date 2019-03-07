// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;

namespace Iot.Device.SocketCan
{
    internal class Interop
    {
        private const int PF_CAN = 29;

        [DllImport("libc", EntryPoint = "socket", CallingConvention = CallingConvention.Cdecl)]
        private static extern int CreateNativeSocket(int domain, int type, int protocol);

        [DllImport("libc", EntryPoint = "ioctl", CallingConvention = CallingConvention.Cdecl)]
        private static extern int Ioctl3(int fd, uint request, ref ifreq ifr);

        [DllImport("libc", EntryPoint = "bind", CallingConvention = CallingConvention.Cdecl)]
        private static extern int BindSocket(int fd, ref sockaddr_can addr, uint addrlen);

        [DllImport("libc", EntryPoint = "close", CallingConvention = CallingConvention.Cdecl)]
        private static extern int CloseSocket(int fd);

        [DllImport("libc", EntryPoint = "write", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        private static unsafe extern int SocketWrite(int fd, byte* buffer, int size);

        [DllImport("libc", EntryPoint = "read", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        private static unsafe extern int SocketRead(int fd, byte* buffer, int size);

        public static unsafe void Write(SafeHandle handle, ReadOnlySpan<byte> buffer)
        {
            fixed (byte* b = buffer)
            {
                while (buffer.Length > 0)
                {
                    int bytesWritten = Interop.SocketWrite((int)handle.DangerousGetHandle(), b, buffer.Length);
                    if (bytesWritten < 0)
                    {
                        throw new IOException("`write` operation failed");
                    }

                    buffer = buffer.Slice(bytesWritten);
                }
            }
        }

        public static unsafe int Read(SafeHandle handle, Span<byte> buffer)
        {
            fixed (byte* b = buffer)
            {
                int bytesRead = Interop.SocketRead((int)handle.DangerousGetHandle(), b, buffer.Length);
                if (bytesRead < 0)
                {
                    throw new IOException("`read` operation failed");
                }

                return bytesRead;
            }
        }

        public static void CloseSocket(IntPtr fd)
        {
            CloseSocket((int)fd);
        }

        public static IntPtr CreateCanRawSocket(string networkInterface)
        {
            const int SOCK_RAW = 3;
            const int CAN_RAW = 1;
            int socket = CreateNativeSocket(PF_CAN, SOCK_RAW, CAN_RAW);

            if (socket == -1)
                throw new IOException("CAN socket could not be created");

            BindToInterface(socket, networkInterface);

            return new IntPtr(socket);
        }

        private static unsafe void BindToInterface(int fd, string interfaceName)
        {
            int idx = GetInterfaceIndex(fd, interfaceName);
            sockaddr_can addr = new sockaddr_can();
            addr.can_family = PF_CAN;
            addr.can_ifindex = idx;

            if (-1 == BindSocket(fd, ref addr, (uint)Marshal.SizeOf<sockaddr_can>()))
            {
                throw new IOException($"Cannot bind to socket to `{interfaceName}`");
            }
        }

        private static unsafe int GetInterfaceIndex(int fd, string name)
        {
            const uint SIOCGIFINDEX = 0x8933;
            const int MaxLen = ifreq.IFNAMSIZ - 1;

            if (Encoding.ASCII.GetByteCount(name) >= MaxLen)
            {
                throw new ArgumentException($"`{name}` exceeds maximum allowed length of {MaxLen} size", nameof(name));
            }

            ifreq ifr = new ifreq();
            fixed (char* inIntefaceName = name)
            {
                int written = Encoding.ASCII.GetBytes(inIntefaceName, name.Length, ifr.ifr_name, MaxLen);
                ifr.ifr_name[written] = 0;
            }

            int ret = Ioctl3(fd, SIOCGIFINDEX, ref ifr);
            if (ret == -1)
            {
                throw new IOException($"Could not get interface index for `{name}`");
            }

            return ifr.ifr_ifindex;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct ifreq
        {
            internal const int IFNAMSIZ = 16;
            public fixed byte ifr_name[IFNAMSIZ];
            public int ifr_ifindex;
            private fixed byte _padding[IFNAMSIZ - sizeof(int)];

        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct sockaddr_can
        {
            public short can_family;
            public int can_ifindex;
            public uint rx_id;
            public uint tx_id;
        }
    }
}
