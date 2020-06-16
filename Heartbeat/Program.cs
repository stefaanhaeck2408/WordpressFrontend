using RabbitMQ.Client;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Heartbeat
{
    class Program
    {
        private static ConnectionFactory factory;

        static void Main(string[] args)
        {
            factory = new ConnectionFactory()
            {
                HostName = "192.168.1.2",
                Port = /*AmqpTcpEndpoint.UseDefaultPort*/ 5672,
                UserName = "frontend_user",
                Password = "frontend_pwd"
            };
            //Start service
            Heartbeat();
        }

        private static void Heartbeat()
        {
            TimerCallback callback = HeartBeatCall;
            Timer timer = new Timer(HeartBeatCall, "test", 500, 500);
            Console.WriteLine("Press any key to exit the sample");
            Console.ReadLine();

        }

        private static void HeartBeatCall(object objectInfo)
        {
            var datetime = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fff");

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

            var xmlResponse = XmlAndXsdValidation(xml);

            if (xmlResponse != null)
            {
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    var addUserBody = Encoding.UTF8.GetBytes(xml);
                    channel.BasicPublish(exchange: "heartbeats.exchange",
                                     routingKey: "",
                                     body: addUserBody
                                     );

                    Console.WriteLine("Heartbeat posted!");
                }
            }
        }

        private static string XmlAndXsdValidation(string objectThatNeedsValidation)
        {
            //XML validation with XSD

            //Select the xsd file
            XDocument xDoc = XDocument.Parse(objectThatNeedsValidation);
            string xsdData;
            string rootname = xDoc.Root.Name.ToString();
            if (rootname == "heartbeat")
            {
                xsdData = @"<?xml version='1.0'?> 
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
        }
    }
}
