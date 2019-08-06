using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.WebSockets;
using System.Threading;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace WpfForWebSocket
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        static readonly Random Rnd = new Random();
        private static Timer _timer;
        private static double _count;



        ClientWebSocket _webSocket = null;
        CancellationToken _cancellation = new CancellationToken();

        bool receiveMessage = false;

        

        public MainWindow()
        {
            InitializeComponent();


            this.Loaded += MainWindow_Loaded;
            Connectserver.IsEnabled = true;
            sendmessage.IsEnabled = false;
            stopsendmessage.IsEnabled = false;
            closeservce.IsEnabled = false;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private async void Connectserver_Click(object sender, RoutedEventArgs e)
        {
            //var url = "ws://localhost:60365/ws";

            //var url1 = "ws://localhost/ws";

            _webSocket = new ClientWebSocket();

            await _webSocket.ConnectAsync(new Uri(IP.Text), _cancellation);

            receiveMessage = true;

            ReciveMessage();

            outmegs.Text = "Connected with server";

            Connectserver.IsEnabled = false;
            sendmessage.IsEnabled = true;
            stopsendmessage.IsEnabled = false;
            closeservce.IsEnabled = true;
        }

        private async void Closeservce_Click(object sender, RoutedEventArgs e)
        {
            receiveMessage = false;
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "NormalClosure", CancellationToken.None);
            _webSocket.Dispose();

            Connectserver.IsEnabled = false;
            sendmessage.IsEnabled = false;
            stopsendmessage.IsEnabled = false;
            closeservce.IsEnabled = false;
        }

        private void Sendmessage_Click(object sender, RoutedEventArgs e)
        {
            _timer = new Timer(DoWork);
            _timer.Change(0, 1000);
            Console.WriteLine("Timer start");


            Connectserver.IsEnabled = false;
            sendmessage.IsEnabled = false;
            stopsendmessage.IsEnabled = true;
            closeservce.IsEnabled = false;
            //var messageBytes = System.Text.Encoding.UTF8.GetBytes(message.Text);
            //await _webSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Binary, true, _cancellation);
        }

        #region RandomMessage

        private async void DoWork(object state)
        {
            _count++;

            Random Rnd = new Random();

            double temp = Rnd.Next(100, 700) / 10.0;

            TemperatureSensor p = new TemperatureSensor() { megnumber = _count.ToString() };

            if (temp > 40)
                p.alerttype = "overheating";
            else
                p.alerttype = "normal";

            p.timestamp = DateTime.Now.ToString();

            p.temperature = temp;

            string json = new JsonConverterHelper().Object2Json(p, typeof(TemperatureSensor));

            Console.WriteLine("Message #" + _count + " " + json);

            var messageBytes = System.Text.Encoding.UTF8.GetBytes(json);

            if (_webSocket != null)
                await _webSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Binary, true, _cancellation);


            this.Dispatcher.Invoke(() =>
            {
                outmegs.Text += new StringBuilder().AppendLine("Message sent " + json).ToString();

                sv.ScrollToVerticalOffset(outmegs.ActualHeight);
            });

            Console.WriteLine("Message sent " + json);
        }


        CancellationTokenSource tokenSource = null;

        public void ChangeUI()
        {
            if (tokenSource != null)
                tokenSource.Cancel();

            tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;

            var task = Task.Factory.StartNew(() =>
            {
                //Do DO Change UI to Rad


                System.Threading.Thread.Sleep(3000);
                if (token.IsCancellationRequested)
                {
                    Console.WriteLine("Abort mission success!");
                    return;
                }

                //TO DO Change UI to Normal

            }, token);

            token.Register(() =>
            {
                Console.WriteLine("Canceled");
            });

        }

        [DataContract]
        internal class TemperatureSensor
        {
            [DataMember]
            internal string megnumber;

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



        private async void ReciveMessage()
        {
            await Task.Run(async () =>
             {
                 try
                 {
                     while (receiveMessage)
                     {
                         var buffer = new byte[1024 * 4];

                         WebSocketReceiveResult result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), new CancellationToken());

                         var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                         this.Dispatcher.Invoke(() =>
                         {
                             Inputmegs.Text += new StringBuilder().AppendLine("recive message: " + message).ToString();

                             sv2.ScrollToVerticalOffset(Inputmegs.ActualHeight);
                             //Inputmegs.Text = message;
                         });

                     }
                 }
                 catch (Exception ex)
                 {
                     Console.WriteLine(ex.Message);
                 }
             });
        }

        private void Stopsendmessage_Click(object sender, RoutedEventArgs e)
        {
            _timer.Dispose();

            Connectserver.IsEnabled = false;
            sendmessage.IsEnabled = true;
            stopsendmessage.IsEnabled = false;
            closeservce.IsEnabled = true;
        }
    }
}
