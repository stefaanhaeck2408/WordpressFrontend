using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMqReceiver.Models;
using WordpressApi;
using WordpressApi.Models;
using WordPressPCL;
using WordPressPCL.Models;

namespace RabbitMqReceiver
{
    class Program
    {
        private static ConnectionFactory factory;
        private static string addUserXsd;
        private static string patchUserXsd;
        private static string errorrXsd;
        private static string heartbeatXsd;
        private static string logXsd;


        private static void Settings() {
            factory = new ConnectionFactory()
            {
                HostName = "192.168.1.2",
                Port = /*AmqpTcpEndpoint.UseDefaultPort*/ 5672,
                UserName = "frontend_user",
                Password = "frontend_pwd"
            };

            addUserXsd = @"<?xml version='1.0'?> 
                                   <xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
                                    <xs:element name='add_user'> 
                                     <xs:complexType> 
                                      <xs:sequence>
                                        <xs:element name='application_name' type='xs: string'/>
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
            patchUserXsd = @"<?xml version='1.0'?> 
                               <xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
                                <xs:element name='patch_user'> 
                                 <xs:complexType> 
                                  <xs:sequence>
                                    <xs:element name='application_name' type='xs: string'/>
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
            errorrXsd = @"<?xml version='1.0'?>
                        < xs:schema xmlns:xs = 'http://www.w3.org/2001/XMLSchema' > 
                            < xs:element name = 'error' >  
                                < xs:complexType >   
                                    < xs:sequence >    
                                        < xs:element name = 'application_name' type = 'xs:string' />       
                                        < xs:element name = 'timestamp' type = 'xs:string' />          
                                        < xs:element name = 'message' type = 'xs:string' />             
                                    </ xs:sequence >              
                                </ xs:complexType >               
                            </ xs:element >
                        </ xs:schema >"; 
            heartbeatXsd = @"<?xml version='1.0'?> 
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
            logXsd = @"<?xml version='1.0'?> 
                    <xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'> 
                     <xs:element name='log'> 
                      <xs:complexType> 
                       <xs:sequence> 
                        <xs:element name='application_name' type='xs:string'/> 
                        <xs:element name='timestamp' type='xs:string'/> 
                        <xs:element name='message' type='xs:string'/> 
                       </xs:sequence> 
                      </xs:complexType> 
                     </xs:element> 
                    </xs:schema>";
        }
        static void Main(string[] args)
        {
            Settings();
            //still have to update the uuid with our userid
            //ReceiverRabbitMQ();
            //Heartbeat();
            //SendLogToLogExchange(" test");

        }

        private static void SendLogToLogExchange(string action)
        {
            //make log file entity
            Log log = new Log("Frontend " + action);
            
            //Make an XML from the object
            XmlSerializer xmlSerializer = new XmlSerializer(log.GetType());
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            string xml;
            var settings = new XmlWriterSettings { Encoding = Encoding.UTF8, Indent = true };
            var stringBuilder = new StringBuilder();
            using (var sww = new ExtendedStringWriter(stringBuilder, Encoding.UTF8))
            {
                using (XmlWriter writer = XmlWriter.Create(sww, settings))
                {
                    xmlSerializer.Serialize(writer, log, ns);
                    xml = sww.ToString();
                }
            }

            //Validate XML
            var xmlResponse = XmlAndXsdValidation(xml);

            //when no errors send the message to rabbitmq
            if (xmlResponse != null)
            {
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    var addUserBody = Encoding.UTF8.GetBytes(xml);
                    channel.BasicPublish(exchange: "logs.exchange",
                                     routingKey: "",
                                     body: addUserBody
                                     );

                }
            }

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

            var xmlResponse = XmlAndXsdValidation(xml);

            if (xmlResponse != null) {
                
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

        
        private static void ReceiverRabbitMQ()
        {
            /*var factory = new ConnectionFactory()
                            {
                                HostName = "192.168.1.2",
                                Port = /*AmqpTcpEndpoint.UseDefaultPort 5672,
                                UserName = "frontend_user",
                                Password = "frontend_pwd"

                            };*/
            try
            {
                //Make the connection to receive
                 
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
                        /*string xsdData =
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
                        });*/

                        var xmlValidationResponse = XmlAndXsdValidation(xml);

                        //When no errors in validation make an object out of the xml data
                        if (xmlValidationResponse != null)
                        {
                            if (xmlValidationResponse == "add_user") {
                                await ReceivingNewUserAsync(xml);
                            }else if(xmlValidationResponse == "patch_user"){
                                await ReceivingPatchUserAsync(xml);
                            }

                            /*
                            XmlSerializer serializer = new XmlSerializer(typeof(AddUser));

                            AddUser receivedUser;

                            using (StringReader reader = new StringReader(xml))
                            {
                                receivedUser = (AddUser)serializer.Deserialize(reader);

                                //Check if user is coming from frontend or other place
                                if (await CheckIfUserIsComingFromOutside(receivedUser.uuid))
                                {

                                    //Connect to wordpress
                                    var client = new WordPressClient("http://127.0.0.1/wordpress/wp-json");
                                    client.AuthMethod = WordPressPCL.Models.AuthMethod.JWT;
                                    await client.RequestJWToken("stefaan.haeck@student.ehb.be", "integration");

                                    //Make collection of meta data
                                    Dictionary<string, string> metadata = new Dictionary<string, string>();
                                                                
                                    metadata.Add("name", receivedUser.name);
                                    metadata.Add("street", receivedUser.street);
                                    metadata.Add("municipal", receivedUser.municipal);
                                    metadata.Add("postal_code", receivedUser.postalCode);
                                    metadata.Add("vat", receivedUser.vat);


                                    if (await client.IsValidJWToken())
                                    {
                                        var user = new User
                                        {
                                            UserName = receivedUser.name,
                                            Email = receivedUser.email,
                                            Password = receivedUser.name + "pass",
                                            Meta = metadata
                                        };

                                        var responseUser = await client.Users.Create(user);

                                        //Create body for requesting UUID for the addUser object
                                        var bodyUuid = new Dictionary<string, string>();
                                        bodyUuid.Add("frontend", responseUser.Id.ToString());

                                        //Make the call to patch the UUID                                    
                                        string responseBody = null;
                                        string url = "http://192.168.1.2/uuid-master/uuids/" + receivedUser.uuid;
                                        using (var clientUuid = new HttpClient())
                                        using (var request = new HttpRequestMessage(HttpMethod.Patch, url))
                                        using (var httpContent = CreateHttpContent(body))
                                        {
                                            request.Content = httpContent;

                                            using (var response = await clientUuid
                                                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead)
                                                .ConfigureAwait(false))
                                            {
                                                response.EnsureSuccessStatusCode();
                                                responseBody = await response.Content.ReadAsStringAsync();
                                            }
                                        }
                                    }

                                }
                            }*/
                        }
                    
                    };
                    channel.BasicConsume(queue: "frontend.queue",
                                             autoAck: true,
                                             consumer: consumer);

                    Console.WriteLine(" Press [enter] to exit.");
                    Console.ReadLine();
                }
            }
            catch (Exception ex) {
                SendErrorMessage(ex);
            }
        }

        private static void SendErrorMessage(Exception ex)
        {
            CustomError error = new CustomError();
            error.application_name = "frontend";
            error.timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff");
            error.message = ex.ToString();

            //Make an XML from the error object
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(CustomError));
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            string xml;
            var settings = new XmlWriterSettings { Encoding = Encoding.UTF8, Indent = true };
            var stringBuilder = new StringBuilder();
            using (var sww = new ExtendedStringWriter(stringBuilder, Encoding.UTF8))
            {
                using (XmlWriter writer = XmlWriter.Create(sww, settings))
                {
                    xmlSerializer.Serialize(writer, error, ns);
                    xml = sww.ToString();
                }
            }

            //XML validation with XSD
            var xmlValidationResponse = XmlAndXsdValidation(xml);

            if (xmlValidationResponse != null)
            {
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    var addUserBody = Encoding.UTF8.GetBytes(xml);
                    channel.BasicPublish(exchange: "logs.exchange",
                                     routingKey: "",
                                     body: addUserBody
                                     );
                }
            }
        }

        private static async System.Threading.Tasks.Task ReceivingNewUserAsync(string xml) {
            XmlSerializer serializer = new XmlSerializer(typeof(AddUser));

            AddUser receivedUser;

            using (StringReader reader = new StringReader(xml))
            {
                receivedUser = (AddUser)serializer.Deserialize(reader);

                //Check if user is coming from frontend or other place
                if (await CheckIfUserIsComingFromOutside(receivedUser.uuid))
                {
                    //Connect to wordpress
                    var client = new WordPressClient("http://127.0.0.1/wordpress/wp-json");
                    client.AuthMethod = WordPressPCL.Models.AuthMethod.JWT;
                    await client.RequestJWToken("stefaan.haeck@student.ehb.be", "integration");

                    //Make collection of meta data
                    Dictionary<string, string> metadata = new Dictionary<string, string>();

                    metadata.Add("name", receivedUser.name);
                    metadata.Add("street", receivedUser.street);
                    metadata.Add("municipal", receivedUser.municipal);
                    metadata.Add("postal_code", receivedUser.postalCode);
                    metadata.Add("vat", receivedUser.vat);

                    if (await client.IsValidJWToken())
                    {
                        var user = new User
                        {
                            UserName = receivedUser.name,
                            Email = receivedUser.email,
                            Password = receivedUser.name + "pass",
                            Meta = metadata
                        };

                        var responseUser = await client.Users.Create(user);

                        //Create body for requesting UUID for the addUser object
                        var bodyUuid = new Dictionary<string, string>();
                        bodyUuid.Add("frontend", responseUser.Id.ToString());

                        //Make the call to patch the UUID                                    
                        string responseBody = null;
                        string url = "http://192.168.1.2/uuid-master/uuids/" + receivedUser.uuid;
                        using (var clientUuid = new HttpClient())
                        using (var request = new HttpRequestMessage(HttpMethod.Patch, url))
                        using (var httpContent = CreateHttpContent(xml))
                        {
                            request.Content = httpContent;

                            using (var response = await clientUuid
                                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead)
                                .ConfigureAwait(false))
                            {
                                response.EnsureSuccessStatusCode();
                                responseBody = await response.Content.ReadAsStringAsync();

                                SendLogToLogExchange(" received new user and added the user");
                            }
                        }
                    }
                }
            }
        }

        private static async System.Threading.Tasks.Task ReceivingPatchUserAsync(string xml) {
            XmlSerializer serializer = new XmlSerializer(typeof(PatchUser));

            PatchUser receivedUser;

            using (StringReader reader = new StringReader(xml))
            {
                receivedUser = (PatchUser)serializer.Deserialize(reader);

                //Make request for get /uuids/uuid
                string responseUuid = null;
                string url = "http://192.168.1.2/uuids" + receivedUser.uuid;
                using (var httpClient = new HttpClient())
                using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                {

                    using (var responseUuidHttpClient = await httpClient
                        .SendAsync(request, HttpCompletionOption.ResponseHeadersRead)
                        .ConfigureAwait(false))
                    {
                        responseUuidHttpClient.EnsureSuccessStatusCode();
                        responseUuid = await responseUuidHttpClient.Content.ReadAsStringAsync();

                        SendLogToLogExchange(" received patch user and updated the user");
                    }
                }

                //Convert the response from json to dictionary
                var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseUuid);


                //Connect to wordpress
                var client = new WordPressClient("http://127.0.0.1/wordpress/wp-json");
                client.AuthMethod = WordPressPCL.Models.AuthMethod.JWT;
                await client.RequestJWToken("stefaan.haeck@student.ehb.be", "integration");

                //Get user with userId retrieved from uuid
                var user = await client.Users.GetByID(values["frontend"]);

                //make meta fields
                Dictionary<string, string> metadata = new Dictionary<string, string>();

                if (receivedUser.street != null)
                {
                    metadata.Add("street", receivedUser.street);
                }
                if (receivedUser.name != null)
                {
                    metadata.Add("name", receivedUser.name);
                }
                if (receivedUser.municipal != null)
                {
                    metadata.Add("municipal", receivedUser.municipal);
                }
                if (receivedUser.postalCode != null)
                {
                    metadata.Add("postal_code", receivedUser.postalCode);
                }
                if (receivedUser.vat != null)
                {
                    metadata.Add("vat", receivedUser.vat);
                }

                //Update user values
                if (receivedUser.email != "")
                {
                    user.UserName = receivedUser.email;
                    user.Email = receivedUser.email;
                    user.Meta = metadata;
                }
                var response = await client.Users.Update(user);
            }        
        }

        private static string XmlAndXsdValidation(string objectThatNeedsValidation)
        {
            //XML validation with XSD

            //Select the xsd file
            XDocument xDoc = XDocument.Parse(objectThatNeedsValidation);
            string xsdData;
            string rootname = xDoc.Root.Name.ToString();
            if (rootname == "patch_user")
            {
                xsdData = patchUserXsd;
            }
            else if (rootname == "add_user")
            {
                xsdData = addUserXsd;
            }
            else if (rootname == "heartbeat")
            {
                xsdData = heartbeatXsd;
            }
            else if (rootname == "error")
            {
                xsdData = errorrXsd;
            }
            else if (rootname == "log") {
                xsdData = logXsd;
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

        private static HttpContent CreateHttpContent(object content)
        {

            HttpContent httpContent = null;

            if (content != null)
            {
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
        /*
        private static void ReceiverRabbitMQPatchUsers()
        {
            //Make the connection to receive users
            var factory = new ConnectionFactory()
            {
                HostName = "192.168.1.2",
                Port = /*AmqpTcpEndpoint.UseDefaultPort 5672,
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
                                <xs:element name='patch_user'> 
                                 <xs:complexType> 
                                  <xs:sequence>
                                    <xs:element name='application_name' type='xs: string'/>
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
                    if (!errors)
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(PatchUser));

                        PatchUser receivedUser;

                        using (StringReader reader = new StringReader(xml))
                        {
                            receivedUser = (PatchUser)serializer.Deserialize(reader);

                            //Make request for get /uuids/uuid
                            string responseUuid = null;
                            string url = "http://192.168.1.2/uuids" + receivedUser.uuid;
                            using (var httpClient = new HttpClient())                                
                            using (var request = new HttpRequestMessage(HttpMethod.Get, url))                            
                            {                               

                                using (var responseUuidHttpClient = await httpClient
                                    .SendAsync(request, HttpCompletionOption.ResponseHeadersRead)
                                    .ConfigureAwait(false))
                                {
                                    responseUuidHttpClient.EnsureSuccessStatusCode();
                                    responseUuid = await responseUuidHttpClient.Content.ReadAsStringAsync();
                                }
                            }

                            //Convert the response from json to dictionary
                            var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseUuid);


                            //Connect to wordpress
                            var client = new WordPressClient("http://127.0.0.1/wordpress/wp-json");
                                client.AuthMethod = WordPressPCL.Models.AuthMethod.JWT;
                                await client.RequestJWToken("stefaan.haeck@student.ehb.be", "integration");

                            //Get user with userId retrieved from uuid
                            var user = await client.Users.GetByID(values["frontend"]);

                            //make meta fields
                            Dictionary<string, string> metadata = new Dictionary<string, string>();

                            if (receivedUser.street != null) {
                                metadata.Add("street", receivedUser.street);
                            }
                            if (receivedUser.name != null)
                            {
                                metadata.Add("name", receivedUser.name);
                            }
                            if (receivedUser.municipal != null)
                            {
                                metadata.Add("municipal", receivedUser.municipal);
                            }
                            if (receivedUser.postalCode != null)
                            {
                                metadata.Add("postal_code", receivedUser.postalCode);
                            }
                            if (receivedUser.vat != null)
                            {
                                metadata.Add("vat", receivedUser.vat);
                            }

                            //Update user values
                            if (receivedUser.email != "") {
                                user.UserName = receivedUser.email;
                                user.Email = receivedUser.email;
                                user.Meta = metadata; 
                            }
                            var response = await client.Users.Update(user);
                        }
                    };
                    channel.BasicConsume(queue: "frontend.queue",
                                         autoAck: true,
                                         consumer: consumer);

                    Console.WriteLine(" Press [enter] to exit.");
                    Console.ReadLine();
                };
            }
        }*/

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

        
    
