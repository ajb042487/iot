﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Device.Spi
{
    /// <summary>
    /// Class that holds the connection settings of a device on a Spi bus.
    /// </summary>
    public sealed class SpiConnectionSettings
    {
        private const SpiMode _defaultSpiMode = SpiMode.Mode0;
        private const int _defaultDataBitLenght = 8; // 1 byte.
        private const int _defaultClockFrequency = 500_000; // 500 KHz

        private SpiConnectionSettings() { }

        /// <summary>
        /// Default constructor. Takes the bus id and the chip select line for that bus.
        /// </summary>
        /// <param name="busId">The bus id the device is connected to.</param>
        /// <param name="chipSelectLine">The chip select line used on that bus id.</param>
        public SpiConnectionSettings(int busId, int chipSelectLine)
        {
            BusId = busId;
            ChipSelectLine = chipSelectLine;
            Mode = _defaultSpiMode;
            DataBitLength = _defaultDataBitLenght;
            ClockFrequency = _defaultClockFrequency;
        }

        /// <summary>
        /// The Spi mode being used.
        /// </summary>
        public SpiMode Mode { get; set; }
        /// <summary>
        /// The bus id the device is connected to.
        /// </summary>
        public int BusId { get; set; }
        /// <summary>
        /// The chip select line used on that bus id.
        /// </summary>
        public int ChipSelectLine { get; set; }
        /// <summary>
        /// The length of the data to be transfered.
        /// </summary>
        public int DataBitLength { get; set; }
        /// <summary>
        /// The frequency in which the data will be transfered.
        /// </summary>
        public int ClockFrequency { get; set; }
    }
}
