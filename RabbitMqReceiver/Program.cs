using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using WordpressApi.Models;

namespace RabbitMqReceiver
{
    class Program
    {
        static void Main(string[] args)
        {
            ReceiverRabbitMQUsers();
            //Heartbeat();



        }

        private static void Heartbeat() {
            TimerCallback callback = HeartBeatCall;
            Timer timer = new Timer(HeartBeatCall, "test", 500, 500);
            Console.WriteLine("Press any key to exit the sample");
            Console.ReadLine();

        }

        private static void HeartBeatCall(object objectInfo)
        {
            var datetime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff");

            string xml;
            using (var sww = new StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(sww))
                {
                    writer.WriteStartElement("heartbeat");
                    writer.WriteElementString("application_name", "frontend");
                    writer.WriteElementString("timestamp", datetime);
                    writer.WriteEndElement();
                    writer.Flush();
                    xml = sww.ToString();
                }
            }

            //XML validation with XSD
            string xsdData =
                @"<?xml version='1.0'?> 
                    <xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'> 
                     <xs:element name='heartbeat'> 
                      <xs:complexType> 
                       <xs:sequence> 
                        <xs:element name='application_name' type='xs:string'/> 
                        <xs:element name='timestamp' type='xs:string'/> 
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
                    channel.BasicPublish(exchange: "heartbeats.exchange",
                                     routingKey: "",                                     
                                     body: addUserBody
                                     );
                }
            }

            

            

        }

        private static void ReceiverRabbitMQUsers()
        {
            //Make the connection to receive users
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
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += async (model, ea) =>
                {
                    //Receiving data
                    var body = ea.Body.ToArray();
                    var xml = Encoding.UTF8.GetString(body);
                    Console.WriteLine(" [x] Received {0}", xml);

                    //Validate received xml
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

                    //When no errors in validation make an object out of the xml data
                    if (!errors) { 
                        XmlSerializer serializer = new XmlSerializer(typeof(AddUser));

                        AddUser receivedUser;

                        using (StringReader reader = new StringReader(xml)) {
                            receivedUser = (AddUser)serializer.Deserialize(reader);

                            //Check if user is coming from frontend or other place
                            if (await CheckIfUserIsComingFromOutside(receivedUser.uuid)) { 
                                
                            }

                        }
                    }
                };
                channel.BasicConsume(queue: "frontend.queue",
                                     autoAck: true,
                                     consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }

        private static async System.Threading.Tasks.Task<bool> CheckIfUserIsComingFromOutside(string uuid)
        {
            var url = "http://192.168.1.2/uuids/" + uuid;
            var responseBody = "";
            using (var client = new HttpClient())
                
            using (var request = new HttpRequestMessage(HttpMethod.Get, url))            
            {  
                using (var response = await client
                    .SendAsync(request, HttpCompletionOption.ResponseHeadersRead)
                    .ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();
                    responseBody = await response.Content.ReadAsStringAsync();
                }
            }
            if (responseBody.Contains("frontend"))
            {
                return false;
            }
            else {
                return true;
            }
            
            
        }
    }
}

        
    
