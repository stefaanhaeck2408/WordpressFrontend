using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using WordpressApi.Models;

namespace WordpressApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        [HttpPost]
        public StatusCodeResult AddEvent([FromBody]Object json)
        {
            var addEventEntity = new AddEvent(json.ToString());

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(AddEvent));
            var xml = "";

            using (var sww = new StringWriter()) {
                using (XmlWriter writer = XmlWriter.Create(sww)) {
                    xmlSerializer.Serialize(writer, addEventEntity);
                    xml = sww.ToString();
                }
            }
            
            var factory = new ConnectionFactory() { HostName = "http://10.3.50.9:5672" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var message = json.ToString();
                var body = Encoding.UTF8.GetBytes(xml);
                var properties = channel.CreateBasicProperties();
                properties.Headers = new Dictionary<string, object>();
                properties.Headers.Add("eventType", "addEvent");                
                channel.BasicPublish(exchange: "events.exchange",
                                     routingKey: "",
                                     basicProperties: properties,
                                     body: body
                                     );                
            }

            return StatusCode(201);
        }

        [HttpPost]
        public StatusCodeResult AddUserToEvent([FromBody]Object json)
        {
            
            return StatusCode(201);
        }
    }
}