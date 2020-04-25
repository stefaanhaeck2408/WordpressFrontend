using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
            //convert json to addUser object
            var addUserEntity = new AddUser(json.ToString());

            //Create body for requesting UUID for the addUser object
            var body = new Dictionary<string, string>();
            body.Add("frontend", addUserEntity.UserId.ToString());

            //Make the call for the UUID
            // https://johnthiriet.com/efficient-post-calls/
            string responseBody = null;
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage(HttpMethod.Post, "http://192.168.1.2/uuids"))
            using (var httpContent = CreateHttpContent(body))
            {
                request.Content = httpContent;

                using (var response = await client
                    .SendAsync(request, HttpCompletionOption.ResponseHeadersRead)
                    .ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();
                    responseBody = await response.Content.ReadAsStringAsync();
                }
            }

            //Convert the response from json to dictionary
            var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseBody);

            //add response to addUserEntity uuid
            addUserEntity.uuid = values["uuid"];

            //Make an XML from the addUser object
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(AddUser));
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            string xml;
            var settings = new XmlWriterSettings { Encoding = Encoding.UTF8, Indent = true };
            var stringBuilder = new StringBuilder();
            using (var sww = new ExtendedStringWriter(stringBuilder, Encoding.UTF8))
            {
                using (XmlWriter writer = XmlWriter.Create(sww, settings))
                {
                    xmlSerializer.Serialize(writer, addUserEntity, ns);
                    xml = sww.ToString();
                }
            }

            //XML validation with XSD
            string xsdData =
                @"<?xml version='1.0'?> 
                   <xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
                    <xs:element name='add_user'> 
                     <xs:complexType> 
                      <xs:sequence> 
                        <xs:element name='name' type='xs:string'/> 
                        <xs:element name='uuid' type='xs:string'/> 
                        <xs:element name='email' type='xs:string'/> 
                        <xs:element name='street' type='xs:string'/> 
                        <xs:element name='municipal' type='xs:string'/> 
                        <xs:element name='postalCode' type='xs:string'/> 
                        <xs:element name='vat' type='xs:string'/> 
                      </xs:sequence> 
                     </xs:complexType> 
                    </xs:element> 
                   </xs:schema>";
            XmlSchemaSet schemas = new XmlSchemaSet();
            schemas.Add("", XmlReader.Create(new StringReader(xsdData)));

            var xDoc = XDocument.Parse(xml);
            bool errors = false;
            xDoc.Validate(schemas, (o, e) =>
            {                
                errors = true;
            });

            //when no errors send the message to rabbitmq
            if (!errors) {
                var factory = new ConnectionFactory()
                {                    
                        HostName = "192.168.1.2",
                        Port = /*AmqpTcpEndpoint.UseDefaultPort*/ 5672,
                        UserName = "frontend_user",
                        Password = "frontend_pwd"
                        
                };
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {                    
                    var addUserBody = Encoding.UTF8.GetBytes(xml);
                    var properties = channel.CreateBasicProperties();
                    properties.Headers = new Dictionary<string, object>();
                    properties.Headers.Add("eventType", "frontend.addUser");
                    channel.BasicPublish(exchange: "events.exchange",
                                     routingKey: "",                                     
                                     basicProperties: properties,
                                     body: addUserBody
                                     );
                }
            }
            return StatusCode(201);
        }

        private static HttpContent CreateHttpContent(object content) {

            HttpContent httpContent = null;

            if (content != null) {
                var ms = new MemoryStream();
                SerializeJsonIntoStream(content, ms);
                ms.Seek(0, SeekOrigin.Begin);
                httpContent = new StreamContent(ms);
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }

            return httpContent;
        }

        public static void SerializeJsonIntoStream(object value, Stream stream)
        {
            using (var sw = new StreamWriter(stream, new UTF8Encoding(false), 1024, true))
            using (var jtw = new JsonTextWriter(sw) { Formatting = Newtonsoft.Json.Formatting.None })
            {
                var js = new JsonSerializer();
                js.Serialize(jtw, value);
                jtw.Flush();
            }
        }


        [HttpPatch]
        public StatusCodeResult PatchUser([FromBody]object json)
        {

            return StatusCode(201);
        }

    }
}