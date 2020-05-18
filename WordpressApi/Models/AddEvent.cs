using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json.Linq;

namespace WordpressApi.Models
{
    [XmlRoot(ElementName = "email_event")]
    public class AddEvent
    {
        public string application_name { get; set; }
        public string event_id { get; set; }

        public AddEvent()
        {

        }

        public AddEvent(string json)
        {
            JObject jObject = JObject.Parse(json);
            JToken jEvent = jObject;
            event_id = jEvent["EventId"].First.ToString();
            application_name = "frontend";
        }
    }
}
