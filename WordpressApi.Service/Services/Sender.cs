using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using WordpressApi.DAL.Models;

namespace WordpressApi.Service.Services
{
    public class Sender
    {
        private static ConnectionFactory factory;

        public static Boolean SendErrorMessage(Exception ex) {

            factory = new ConnectionFactory()
            {
                HostName = "192.168.1.2",
                Port = /*AmqpTcpEndpoint.UseDefaultPort*/ 5672,
                UserName = "frontend_user",
                Password = "frontend_pwd"
            };

            CustomError customError = new CustomError();
            customError.application_name = "frontend";
            customError.timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff");
            customError.message = ex.ToString();

            string xmlResponse = XsdValidation.XmlObjectValidation(customError);

            if (xmlResponse != null)
                {
                    using (var connection = factory.CreateConnection())
                    using (var channel = connection.CreateModel())
                    {
                        var addUserBody = Encoding.UTF8.GetBytes(xmlResponse);
                        channel.BasicPublish(exchange: "errors.exchange",
                                         routingKey: "",
                                         body: addUserBody
                                         );
                    }

                return true;
            }
            return false;
        }
    }
}
