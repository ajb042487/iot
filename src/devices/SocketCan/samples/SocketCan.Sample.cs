// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Linq;
using System.Threading;

namespace Iot.Device.SocketCan.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            byte[] payload = new byte[16]
            {
                0x1A, 0, 0, 0, // id
                8, // len
                0, // flags
                0, 0, // reserved
                1, 2, 3, 4, 5, 6, 7, 8
            };
            using (CanRawStream stream = new CanRawStream())
            {
                //stream.Write(payload);
                //stream.ReadTest();
                stream.Filter(0x1A);
                stream.Listen(new Listener());
            }
        }
    }

    class Listener : ICanRawListener
    {
        private byte[] _buffer = new byte[72];

        public byte[] GetBuffer(int minLength) => _buffer;
        public void FrameReceived(uint address, CanFlags flags, ReadOnlySpan<byte> data)
        {
            Console.WriteLine($"Address: {address.ToString("X2")}; Flags: {flags}; {string.Join("", data.ToArray().Select((x) => x.ToString("X2")))}");
        }
    }
}
