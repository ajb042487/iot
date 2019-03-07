# SocketCan

## What will you need

- MCP2515
- Raspberry PI

## Instructions for MCP2515 for Raspberry PI

Connect SPI device to regular SPI pins.
Interrupt pin should be connected to a GPIO pin i.e. `BCM 25`.

Your boot.config should contain following (adjust for your interrup pin and frequency based on quartz on your board):

NOTE: Some of the following instructions might not be needed.

```
dtparam=spi=on
dtoverlay=mcp2515-can0,oscillator=8000000,interrupt=25
dtoverlay=spi-bcm2835-overlay

# if any issues
# core_freq=250
# core_freq_min=250
```

Run (not reqiored)
```
sudo modprobe can
sudo modprobe can-dev
sudo modprobe can-raw
sudo modprobe mcp251x
```

For test run `ifconfig -a` and check if `can0` (or similar) device is on the list.

Now we need to set network bitrate and connect to it.
Other popular baud rates: 10000, 20000, 50000, 100000, 125000, 250000, 500000, 800000, 1000000

```sh
sudo ip link set can0 up type can bitrate 125000
sudo ifconfig can0 up
```

## Testing the network

These steps are not required but might be useful for diagnosing potential issues.

- Instal can-utils package

```sh
sudo apt-get -y install can-utils
```

- On first device listen to CAN frames

```sh
candump can0
```

- On second device send a packet

```sh
cansend can0 01a#11223344AABBCCDD
```

- On the first device you should see the packet being send by the second device

## References

- https://harrisonsand.com/can-on-the-raspberry-pi/
- http://www.armadeus.org/wiki/index.php?title=CAN_bus_Linux_driver
