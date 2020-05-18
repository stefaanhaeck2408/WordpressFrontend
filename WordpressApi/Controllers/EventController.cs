using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using WordpressApi.DAL.Models;
using WordpressApi.Service.Services;

namespace WordpressApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private static ConnectionFactory factory;        
        public EventController()
        {
            factory = new ConnectionFactory()
            {
                HostName = "192.168.1.2",
                Port = /*AmqpTcpEndpoint.UseDefaultPort*/ 5672,
                UserName = "frontend_user",
                Password = "frontend_pwd"
            };
            
        }
        [HttpPost]
        public StatusCodeResult AddEvent([FromBody]Object json)
        {
            var addEventEntity = new AddEventFromFrontend(json.ToString());

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(AddEventFromFrontend));
            var xml = "";

            using (var sww = new StringWriter()) {
                using (XmlWriter writer = XmlWriter.Create(sww)) {
                    xmlSerializer.Serialize(writer, addEventEntity);
                    xml = sww.ToString();
                }
            }

            //Validate XML
            var xmlResponse = XsdValidation.XmlStringValidation(xml);

            if (xmlResponse != null) {
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    var addUserBody = Encoding.UTF8.GetBytes(xml);
                    var properties = channel.CreateBasicProperties();
                    properties.Headers = new Dictionary<string, object>();
                    properties.Headers.Add("eventType", "frontend.email_event");
                    channel.BasicPublish(exchange: "events.exchange",
                                     routingKey: "",
                                     basicProperties: properties,
                                     body: addUserBody
                                     );

                }
            }
            return StatusCode(201);
        }

        [HttpPost]
        public StatusCodeResult AddUserToEvent([FromBody]Object json)
        {
            
            return StatusCode(201);
        }
        /*
        private static string XmlAndXsdValidation(string objectThatNeedsValidation)
        {
            //XML validation with XSD
            //Select the xsd file
            XDocument xDoc = XDocument.Parse(objectThatNeedsValidation);
            string xsdData;
            string rootname = xDoc.Root.Name.ToString();
            if (rootname == "email_event")
            {
                xsdData = xsdEmailEvent;
            }
            else
            {
                return null;
            }


            XmlSchemaSet schemas = new XmlSchemaSet();
            schemas.Add("", XmlReader.Create(new StringReader(xsdData)));

            //Validation of XML
            //var xDoc = XDocument.Parse(xml);
            bool errors = false;
            xDoc.Validate(schemas, (o, e) =>
            {
                errors = true;
            });

            //Return null when validation has errors
            if (errors)
            {
                return null;
            }
            else
            {
                return rootname;
            }
        }*/
    }
}