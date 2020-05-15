using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace WordpressApi.Models
{
    [XmlRoot(ElementName = "log")]
    public class Log: IXsdValidation
    {
        public string application_name { get; set; }
        public string timestamp { get; set; }
        public string message { get; set; }

        public Log()
        {

        }

        public Log(string text)
        {
            application_name = "frontend";
            timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff");
            message = text;
        }
    }
}
