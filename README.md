# NFApp1

This is a .NET nanoFramework application for ESP32 microcontrollers.

## Description

NFApp1 is a .NET nanoFramework application that demonstrates GPIO interrupt handling and MQTT communication on ESP32 devices. The application monitors a button press, counts the presses, and publishes the count to an MQTT broker at regular intervals.

## Features

- **WiFi Connectivity**: Connects to WiFi network using the nanoFramework networking stack
- **GPIO Interrupt Handling**: Monitors button presses on GPIO pin 25 with debouncing
- **Thread-Safe Counter**: Uses thread-safe operations to track button press counts
- **MQTT Publishing**: Publishes button press data to an MQTT broker (broker.hivemq.com)
- **Real-Time Updates**: Publishes data every 5 seconds when button presses are detected

## Hardware Requirements

- ESP32 development board
- Push button connected to GPIO pin 25 and GND
- WiFi network access

## Software Requirements

- Visual Studio 2019 or later with .NET nanoFramework extension
- .NET nanoFramework firmware flashed on the ESP32

## Setup

1. **Configure WiFi credentials** in `Program.cs`:
   ```csharp
   private const string WifiSsid = "YourWiFiSSID";
   private const string WifiPassword = "YourWiFiPassword";
   ```

2. **Connect hardware**:
   - Connect a push button between GPIO pin 25 and GND
   - The internal pull-up resistor is enabled in code

3. **Build and deploy** the application to your ESP32 device using Visual Studio

## Usage

Once deployed, the application will:
1. Connect to the configured WiFi network
2. Start monitoring the button on GPIO pin 25
3. Count button presses and publish the count to the MQTT topic `device/button/count` every 5 seconds
4. Reset the counter after each publish

Monitor the Debug output to see connection status and button press events.

## MQTT Topic

- **Broker**: broker.hivemq.com
- **Topic**: device/button/count
- **Payload Format**: `{ "presses": <count> }`

## Dependencies

- nanoFramework.CoreLibrary
- nanoFramework.Hardware.Esp32
- nanoFramework.M2Mqtt
- nanoFramework.System.Device.Gpio
- nanoFramework.System.Device.Wifi
- nanoFramework.Networking

See `packages.config` for complete dependency list and versions.