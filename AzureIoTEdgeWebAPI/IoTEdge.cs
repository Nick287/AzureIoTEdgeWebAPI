using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Transport.Mqtt;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;

namespace AzureIoTEdgeWebAPI
{
    public class IoTEdge
    {
        static int counter;

        private static ModuleClient _moduleClient { get; set; }

        static string CloudStorageAccount { get; set; }
        static string ContainerName { get; set; }
        static string PathAndFileName { get; set; }

        public static void IoTEdgeGo()
        {
            Init().Wait();

            // Wait until the app unloads or is cancelled
            var cts = new CancellationTokenSource();
            AssemblyLoadContext.Default.Unloading += (ctx) => cts.Cancel();
            Console.CancelKeyPress += (sender, cpe) => cts.Cancel();
            WhenCancelled(cts.Token).Wait();
        }

        /// <summary>
        /// Handles cleanup operations when app is cancelled or unloads
        /// </summary>
        public static Task WhenCancelled(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
            return tcs.Task;
        }

        /// <summary>
        /// Initializes the ModuleClient and sets up the callback to receive
        /// messages containing temperature information
        /// </summary>
        static async Task Init()
        {
            MqttTransportSettings mqttSetting = new MqttTransportSettings(TransportType.Mqtt_Tcp_Only);
            ITransportSettings[] settings = { mqttSetting };

            // Open a connection to the Edge runtime
            ModuleClient ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);
            await ioTHubModuleClient.OpenAsync();
            Console.WriteLine("IoT Hub module client initialized.");

            // Read the TemperatureThreshold value from the module twin's desired properties
            var moduleTwin = await ioTHubModuleClient.GetTwinAsync();
            await OnDesiredPropertiesUpdate(moduleTwin.Properties.Desired, ioTHubModuleClient);

            // Attach a callback for updates to the module twin's desired properties.
            await ioTHubModuleClient.SetDesiredPropertyUpdateCallbackAsync(OnDesiredPropertiesUpdate, null);

            ioTHubModuleClient.SetConnectionStatusChangesHandler(ConnectionStatusChangeHandler);


            _moduleClient = ioTHubModuleClient;

            #region for test

            //_timer = new Timer(DoWork);
            //_timer.Change(0, 1000);
            //Console.WriteLine("Timer start");

            #endregion

            // Register callback to be called when a message is received by the module
            await ioTHubModuleClient.SetInputMessageHandlerAsync("input1", PipeMessage, ioTHubModuleClient);
        }
        private static void ConnectionStatusChangeHandler(ConnectionStatus status, ConnectionStatusChangeReason reason)
        {
            Console.WriteLine($"Status {status} changed: {reason}");
        }

        static Task OnDesiredPropertiesUpdate(TwinCollection desiredProperties, object userContext)
        {
            try
            {
                Console.WriteLine("Desired property change:");
                Console.WriteLine(JsonConvert.SerializeObject(desiredProperties));

                if (desiredProperties["CloudStorageAccount"] != null)
                    CloudStorageAccount = desiredProperties["CloudStorageAccount"];

                if (desiredProperties["ContainerName"] != null)
                    ContainerName = desiredProperties["ContainerName"];

                if (desiredProperties["PathAndFileName"] != null)
                    PathAndFileName = desiredProperties["PathAndFileName"];

                MemoryStream memoryStream = new CloudStorageHelper(CloudStorageAccount).DownloadFile(ContainerName, PathAndFileName);

                string text = System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());

                StartupWebSocket.SingleStartup.SendMessage("Download file read the file text is: " + text);

                Console.WriteLine("Download file read the file text is: " + text);

            }
            catch (AggregateException ex)
            {
                foreach (Exception exception in ex.InnerExceptions)
                {
                    Console.WriteLine();
                    Console.WriteLine("Error when receiving desired property: {0}", exception);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Error when receiving desired property: {0}", ex.Message);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// This method is called whenever the module is sent a message from the EdgeHub. 
        /// It just pipe the messages without any change.
        /// It prints all the incoming messages.
        /// </summary>
        static async Task<MessageResponse> PipeMessage(Message message, object userContext)
        {
            //int counterValue = Interlocked.Increment(ref counter);

            var moduleClient = userContext as ModuleClient;
            if (moduleClient == null)
            {
                throw new InvalidOperationException("UserContext doesn't contain " + "expected values");
            }

            byte[] messageBytes = message.GetBytes();
            string messageString = Encoding.UTF8.GetString(messageBytes);
            Console.WriteLine($"Received alert message: " + messageString);

            if (!string.IsNullOrEmpty(messageString))
            {
                //var pipeMessage = new Message(messageBytes);
                //foreach (var prop in message.Properties)
                //{
                //    pipeMessage.Properties.Add(prop.Key, prop.Value);
                //}
                //await moduleClient.SendEventAsync("output1", pipeMessage);


               await StartupWebSocket.SingleStartup.SendMessage(messageString);

                Console.WriteLine("Received alert message sent");
            }
            return MessageResponse.Completed;
        }

        public async static void UpLoadMessage(string message)
        {
            if (_moduleClient == null) return;

            if (string.IsNullOrWhiteSpace(message)) return;

            var messageBytes = System.Text.Encoding.UTF8.GetBytes(message);

            Message pipeMessage = new Message(messageBytes);

            await _moduleClient.SendEventAsync("output1", pipeMessage);

            Console.WriteLine("$Send message to Edge Hub: " + message);
        }


        #region for test
        static readonly Random Rnd = new Random();
        private static Timer _timer;
        private static double _count;
        #endregion


        #region RandomMessage

        private async static void DoWork(object state)
        {
            _count++;

            Random Rnd = new Random();

            double temp = Rnd.Next(100, 700) / 10.0;

            TemperatureSensor p = new TemperatureSensor();

            if (temp > 50)
                p.alerttype = "overheating";
            else
                p.alerttype = "normal";

            p.timestamp = DateTime.Now.ToString();

            p.temperature = temp;

            string json = new JsonConverterHelper().Object2Json(p, typeof(TemperatureSensor));

            Console.WriteLine("Message #" + _count + " " + json);

            var messageBytes = System.Text.Encoding.UTF8.GetBytes(json);

            //Encoding.UTF8.GetBytes(messageBody)

            Message pipeMessage = new Message(messageBytes);

            // if (temp > 50)
            //     pipeMessage.Properties.Add("MessageType", "Alert");

            await _moduleClient.SendEventAsync("TemperatureModuleOutput", pipeMessage);

            Console.WriteLine("Message sent " + json);
        }

        [DataContract]
        internal class TemperatureSensor
        {
            [DataMember]
            internal string timestamp;

            [DataMember]
            internal string alerttype;

            [DataMember]
            internal double temperature;
        }

        public class JsonConverterHelper
        {
            public string Object2Json(object obj, Type type)
            {
                string Json = string.Empty;
                MemoryStream stream1 = new MemoryStream();
                DataContractJsonSerializer ser = new DataContractJsonSerializer(type);
                ser.WriteObject(stream1, obj);
                stream1.Position = 0;
                StreamReader sr = new StreamReader(stream1);
                //Console.Write("JSON form of Person object: ");
                return sr.ReadToEnd();
            }

            // public List<Data> GetData(string Json)
            // {
            //     var data = JsonConvert.DeserializeObject<List<Data>>(Json);
            //     return data;
            // }

        }

        #endregion
    }
}
