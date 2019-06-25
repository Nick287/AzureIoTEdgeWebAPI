using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace AzureIoTEdgeWebAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            //SocketClientCore.StartClient();

            //try
            //{
            //    string baseAddress = "http://localhost:9000/";

            //    HttpClient httpClient = new HttpClient();

            //    var requestBody = new HttpRequestMessage()
            //    {
            //        RequestUri = new Uri(baseAddress + "api/values"),
            //        Method = HttpMethod.Post,
            //        Content = new StringContent("{31231:123123}", Encoding.UTF8, "application/json")
            //    };

            //    var response1 = httpClient.SendAsync(requestBody).Result;

            //    Console.WriteLine(response1);
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine();
            //    Console.WriteLine(ex.Message);
            //    Console.WriteLine();
            //}

            return new string[] { "Hello", "Wang Bo" };
        }

        // GET api/values/5
        [HttpGet("{message}")]
        public ActionResult<string> Get(string message)
        {
            string meg = "This message recive from Edge module webapi message is : " + message;

            Console.WriteLine();
            Console.WriteLine(meg);

            //try
            //{
            //    Console.WriteLine();
            //    string localPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            //    string localFileName = "QuickStart_" + Guid.NewGuid().ToString() + ".txt";
            //    string sourceFile = Path.Combine(localPath, localFileName);
            //    // Write text to the file.
            //    System.IO.File.WriteAllText(sourceFile, "Hello, World!");

            //    Console.WriteLine("Temp file = {0}", sourceFile);
            //    Console.WriteLine("WriteAllText successed");
            //    Console.WriteLine();
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("ex message: " + ex.ToString());
            //    Console.WriteLine();
            //}

            try
            {
                Task.Run(() =>
                {
                    IoTEdge.UpLoadMessage(message);
                    //SocketClientCore.StartClient();

                }).Wait();

                return Ok(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("ex message: " + ex.ToString());
                return BadRequest(ex);
            }
        }

        // POST api/values
        [HttpPost]
        public ActionResult<string> Post()
        {
            try
            {
                using (StreamReader reader = new StreamReader(HttpContext.Request.Body, Encoding.UTF8))
                {
                    string message = reader.ReadToEnd();

                    string meg = "This message recive from Edge module webapi message is : " + message;

                    Console.WriteLine();
                    Console.WriteLine(meg);

                    Task.Run(() =>
                    {
                        IoTEdge.UpLoadMessage(message);
                    }).Wait();

                    return Ok(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("ex message: " + ex.ToString());
                return BadRequest(ex);
            }
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
