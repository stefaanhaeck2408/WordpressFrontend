using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace RabbitMqReceiver.Models
{
    [XmlRoot(ElementName = "error")]
    public class CustomError
    {
        public string application_name { get; set; }
        public string timestamp { get; set; }
        public string message { get; set; }

        public CustomError()
        {

        }
    }


}
