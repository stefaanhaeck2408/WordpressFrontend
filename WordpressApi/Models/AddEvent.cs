using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json.Linq;

namespace WordpressApi.Models
{
    [XmlRoot(ElementName = "add_event")]
    public class AddEvent
    {
        public int eventId { get; set; }

        public AddEvent()
        {

        }

        public AddEvent(string json)
        {
            JObject jObject = JObject.Parse(json);
            JToken jEvent = jObject;
            eventId = (int)jEvent["EventId"].First;
        }
    }
}
