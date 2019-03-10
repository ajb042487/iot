// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Iot.Device.SocketCan
{
    public interface ICanRawListener
    {
        // buffer should be at least `length` bytes long
        // length will be no more than 72 bytes
        byte[] GetBuffer(int minLength);
        void FrameReceived(uint address, CanFlags flags, ReadOnlySpan<byte> data);
    }
}
