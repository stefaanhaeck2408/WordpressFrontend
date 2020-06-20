using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace WordpressApi.DAL.Models
{
    [XmlRoot(ElementName = "email_event")]
    public class EmailEvent:  IXsdValidation
    {
        public string application_name { get; set; }
        public string event_id { get; set; }

        public EmailEvent()
        {

        }

        public EmailEvent(string json)
        {
            JObject jObject = JObject.Parse(json);
            JToken jEvent = jObject;
            event_id = jEvent["EventId"].ToString();
            application_name = "frontend";
        }
    }
}
