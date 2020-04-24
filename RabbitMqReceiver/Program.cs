using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMqReceiver
{
    class Program
    {
        static void Main(string[] args)
        {
            //ReceiverRabbitMQUsers();
            Heartbeat();



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
                    Port = 5672,
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
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.Span;
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(" [x] Received {0}", message);
                };
                channel.BasicConsume(queue: "frontend.queue",
                                     autoAck: true,
                                     consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }

    }
}

        
    
