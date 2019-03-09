// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.I2c.Drivers;
using System.Device.Spi;
using System.Device.Spi.Drivers;
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
                stream.Write(payload);
                //stream.ReadTest();
            }
        }
    }
}
