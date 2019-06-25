using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AzureIoTEdgeWebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                Task.Run(() =>
                {
                    Console.WriteLine("go IoTEdgeGo");
                    //IoTEdge.IoTEdgeGo();
                    Console.WriteLine("");
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            //try
            //{
            //    Task.Run(() =>
            //    {
            //        Console.WriteLine("go StartServer");
            //        //MqttHost.StartServer();
            //        //SocketHostCore.StartServer();
            //        //TcpHost.StartServer();
            //        Console.WriteLine("");
            //    });
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //}

            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<StartupWebSocket>();
    }
}
