using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json.Linq;

namespace WordpressApi.Models
{
    [XmlRoot(ElementName = "patch_user")]
    public class UpdateUser
    {
        [XmlIgnoreAttribute]
        public int UserId { get; set; }
        public string name { get; set; }
        public string uuid { get; set; }
        public string email { get; set; }
        public string street { get; set; }
        public string municipal { get; set; }
        public string postalCode { get; set; }
        public string vat { get; set; }

        public UpdateUser()
        {

        }

        public UpdateUser(string json)
        {
            JObject jObject = JObject.Parse(json);
            JToken jUser = jObject;
            name = jUser["Name"].First.ToString();
            email = jUser["Email"].ToString();
            street = jUser["street"].First.ToString();
            municipal = jUser["Municipal"].First.ToString();
            postalCode = jUser["PostalCode"].First.ToString();
            vat = jUser["vat"].First.ToString();
            UserId = (int)jUser["UserId"];
        }
    }
}
