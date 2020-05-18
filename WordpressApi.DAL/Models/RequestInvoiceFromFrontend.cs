using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace WordpressApi.DAL.Models
{
    [XmlRoot(ElementName = "email_invoice")]
    public class RequestInvoiceFromFrontend : IXsdValidation
    {
        [XmlIgnoreAttribute]
        public int UserId { get; set; }
        public string application_name { get; set; }
        public string event_id { get; set; }
        public string uuid { get; set; }

        public RequestInvoiceFromFrontend()
        {
            
        }

        public RequestInvoiceFromFrontend(string json)
        {
            JObject jObject = JObject.Parse(json);
            JToken jInvoice = jObject;
            UserId = (int)jInvoice["UserId"];
            event_id = jInvoice["EventId"].ToString();
            application_name = "frontend";
        }
    }
}
