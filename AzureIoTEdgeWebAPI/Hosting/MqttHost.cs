using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Core;
using MQTTnet.Core.Adapter;
using MQTTnet.Core.Diagnostics;
using MQTTnet.Core.Protocol;
using MQTTnet.Core.Server;
using System.Text;
using System.Threading;

namespace AzureIoTEdgeWebAPI
{

    public class MqttHost
    {

        private static MqttServer mqttServer = null;
        public static void StartServer()
        {
            MqttNetTrace.TraceMessagePublished += MqttNetTrace_TraceMessagePublished;
            new Thread(StartMqttServer).Start();

            while (true)
            {
                var inputString = Console.ReadLine().ToLower().Trim();

                if (inputString == "exit")
                {
                    mqttServer?.StopAsync();
                    Console.WriteLine("MQTT Stoped！");
                    break;
                }
                else if (inputString == "clients")
                {
                    foreach (var item in mqttServer.GetConnectedClients())
                    {
                        Console.WriteLine($"client：{item.ClientId}，protocol version：{item.ProtocolVersion}");
                    }
                }
                else if (inputString == "publish")
                {
                    string topic = "hellobo";
                    string inputString1 = "hello123123123";
                    var appMsg = new MqttApplicationMessage(topic, Encoding.UTF8.GetBytes(inputString1), MqttQualityOfServiceLevel.AtMostOnce, false);
                    mqttServer.Publish(appMsg);
                }
                else
                {
                    Console.WriteLine($"command[{inputString}]Invalid！");
                }

            }
        }

        private static void StartMqttServer()
        {
            if (mqttServer == null)
            {
                try
                {
                    var options = new MqttServerOptions
                    {
                        ConnectionValidator = p =>
                        {
                            if (p.ClientId == "c001")
                            {
                                if (p.Username != "u001" || p.Password != "p001")
                                {
                                    return MqttConnectReturnCode.ConnectionRefusedBadUsernameOrPassword;
                                }
                            }

                            return MqttConnectReturnCode.ConnectionAccepted;
                        }
                    };

                    mqttServer = new MqttServerFactory().CreateMqttServer(options) as MqttServer;
                    mqttServer.ApplicationMessageReceived += MqttServer_ApplicationMessageReceived;
                    mqttServer.ClientConnected += MqttServer_ClientConnected;
                    mqttServer.ClientDisconnected += MqttServer_ClientDisconnected;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return;
                }
            }

            mqttServer.StartAsync();
            Console.WriteLine("MQTT started！");
        }

        private static void MqttServer_ClientConnected(object sender, MqttClientConnectedEventArgs e)
        {
            Console.WriteLine($"client[{e.Client.ClientId}]connect，protocol version：{e.Client.ProtocolVersion}");
        }

        private static void MqttServer_ClientDisconnected(object sender, MqttClientDisconnectedEventArgs e)
        {
            Console.WriteLine($"client[{e.Client.ClientId}] disconnect！");
        }

        private static void MqttServer_ApplicationMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e)
        {
            Console.WriteLine($"client[{e.ClientId}]>> topic：{e.ApplicationMessage.Topic} load：{Encoding.UTF8.GetString(e.ApplicationMessage.Payload)} Qos：{e.ApplicationMessage.QualityOfServiceLevel} 保留：{e.ApplicationMessage.Retain}");
        }

        private static void MqttNetTrace_TraceMessagePublished(object sender, MqttNetTraceMessagePublishedEventArgs e)
        {
            //Console.WriteLine($">> 线程ID：{e.ThreadId} 来源：{e.Source} 跟踪级别：{e.Level} 消息: {e.Message}");

            //if (e.Exception != null)
            //{
            //    Console.WriteLine(e.Exception);
            //}
        }
    }
}
