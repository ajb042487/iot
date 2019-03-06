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
core_freq=250
core_freq_min=250
dtoverlay=mcp2515-can0,oscillator=8000000,interrupt=25
dtoverlay=spi-bcm2835-overlay
```

Run
```
sudo modprobe can
sudo modprobe can-dev
sudo modprobe can-raw
sudo modprobe mcp251x
```

For test run `ifconfig -a` and check if `can0` (or similar) device is on the list.

## References

- https://harrisonsand.com/can-on-the-raspberry-pi/
