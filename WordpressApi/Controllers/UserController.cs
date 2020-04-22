using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using WordpressApi.Models;

namespace WordpressApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        [HttpPost]
        public async Task<StatusCodeResult> AddUserAsync([FromBody]Object json)
        {
            var addUserEntity = new AddUser(json.ToString());

            var body = new Dictionary<string, string>();
            body.Add("frontend", addUserEntity.UserId.ToString());

            HttpClient client = new HttpClient();
            var content = new FormUrlEncodedContent(body);
            var respone = await client.PostAsync("http://192.168.1.2/uuids", content);      
            
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(AddUser));
            var xml = "";

            using (var sww = new StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(sww))
                {
                    xmlSerializer.Serialize(writer, addUserEntity);
                    xml = sww.ToString();
                }
            }

            /*var factory = new ConnectionFactory() { HostName = "http://10.3.50.9:5672" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var message = json.ToString();
                var body = Encoding.UTF8.GetBytes(xml);
                var properties = channel.CreateBasicProperties();
                properties.Headers = new Dictionary<string, object>();
                properties.Headers.Add("eventType", "addUser");
                channel.BasicPublish(exchange: "events.exchange",
                                     routingKey: "",                                     basicProperties: properties,
                                     body: body
                                     );
            }*/

            return StatusCode(201);
        }

        [HttpPatch]
        public StatusCodeResult PatchUser([FromBody]object json)
        {
            return StatusCode(201);
        }

    }
}