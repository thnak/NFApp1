using System;
using System.Device.Gpio;
using System.Device.Wifi;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading;
using nanoFramework.M2Mqtt;
using nanoFramework.Networking;

namespace NFApp1
{
    public abstract class Program
    {
        // The GPIO pin the button is connected to.
        // Connect the other side of the button to GND.
        private const int ButtonPin = 25;

        // --- Thread-Safe Shared Data ---
        // The object we will use for locking to ensure thread safety.
        private static readonly object SLock = new();
        // The counter variable for button presses.
        private static int _sPressCount = 0;
        // ---
        private const string MqttBrokerAddress = "broker.hivemq.com";
        private const string MqttTopic = "device/button/count";

        private const string WifiSsid = "Nghia Dep Trai";
        private const string WifiPassword = "HoiThangKiaDi";

        public static void Main()
        {
            // 0. Connect to WiFi
            Debug.WriteLine("Connecting to WiFi...");
            ConnectToWifi();

            // 1. Setup the GPIO pin for the button
            GpioController gpio = new GpioController();
            
            // Open the pin with an internal pull-up resistor.
            GpioPin buttonPin = gpio.OpenPin(ButtonPin, PinMode.InputPullUp);
            // Set a debounce timeout to filter out noise from the button press.
            buttonPin.DebounceTimeout = TimeSpan.FromMilliseconds(50);
            // Register the event handler method to be called on a pin value change.
            buttonPin.ValueChanged += Button_ValueChanged_Handler;

            Debug.WriteLine("GPIO Interrupt setup complete. Ready for button presses.");

            // Pass MAC address to MQTT thread
            Thread mqttThread = new Thread(() => MqttPublisherThread("sa"));
            mqttThread.Start();

            // 3. Keep the main application thread alive indefinitely
            Thread.Sleep(Timeout.Infinite);
        }

        private static void AvailableNetworksChangedEventHandler(WifiAdapter sender, object e)
        {
            Debug.WriteLine("Available networks changed.");
        }

        /// <summary>
        /// This is our Interrupt Service Routine (ISR).
        /// It's called automatically by the hardware when the button pin's state changes.
        /// It should be VERY FAST.
        /// </summary>
        private static void Button_ValueChanged_Handler(object sender, PinValueChangedEventArgs e)
        {
     
            // We only care about the 'Falling' edge (when the button is pressed and connects the pin to GND).
            if (e.ChangeType == PinEventTypes.Falling)
            {
                // Lock the shared counter to safely increment it.
                lock (SLock)
                {
                    _sPressCount++;
                }
                Debug.WriteLine($"Button pressed! Total count is now: {_sPressCount}");
            }
        }

        /// <summary>
        /// This is the long-running thread that handles publishing the data.
        /// </summary>
        private static void MqttPublisherThread(string clientId)
        {
            MqttClient client = new MqttClient("broker.hivemq.com");
            client.Connect(clientId);
            client.MqttMsgSubscribed += Client_MqttMsgSubscribedHandler;
            Debug.WriteLine("MQTT publisher thread started.");

            while (true)
            {
                // Wait for 5 seconds before publishing.
                Thread.Sleep(5000);

                int countToPublish = 0;

                // Lock the shared counter to safely read its value and reset it.
                lock (SLock)
                {
                    // Only proceed if there's something to report.
                    if (_sPressCount > 0)
                    {
                        countToPublish = _sPressCount;
                        _sPressCount = 0; // Reset the counter after reading
                    }
                }

                // Publish the data if there were any presses in the last interval.
                if (countToPublish > 0)
                {
                    string payload = $"{{ \"presses\": {countToPublish} }}";

                    // convert the payload to a byte array
                    var payloadBytes = System.Text.Encoding.UTF8.GetBytes(payload);
                    client.Publish("device/button/count", payloadBytes);
                    Debug.WriteLine($"Published MQTT message: {payload}");
                }
            }
        }

        private static void Client_MqttMsgSubscribedHandler(object sender, nanoFramework.M2Mqtt.Messages.MqttMsgSubscribedEventArgs e)
        {
            // Optionally handle subscription event
        }

        public static bool ConnectToWifi()
        {

            // Connect the ESP32 Device to the Wi-Fi and check the connection...
            Debug.WriteLine("Program Started, connecting to WiFi.");

            CancellationTokenSource cs = new(60000);
            var success = WifiNetworkHelper.Reconnect(requiresDateTime: true, token: cs.Token);

            if (!success)
            {
                Debug.WriteLine($"Can't connect to wifi: {WifiNetworkHelper.Status}");
                if (WifiNetworkHelper.HelperException != null)
                {
                    Debug.WriteLine($"NetworkHelper.ConnectionError.Exception");
                }
            }

            Debug.WriteLine($"Date and time is now {DateTime.UtcNow}");
            return success;
        }
    }
}