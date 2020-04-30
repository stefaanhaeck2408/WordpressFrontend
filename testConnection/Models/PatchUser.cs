using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Newtonsoft.Json.Linq;

namespace testConnection.Models
{   [XmlRoot(ElementName = "patch_user")]
    class PatchUser
    {
        public string applicationName { get; set; }
        public string name { get; set; }
            public string uuid { get; set; }
            public string email { get; set; }
            public string street { get; set; }
            public string municipal { get; set; }
            public string postalCode { get; set; }
            public string vat { get; set; }
            public PatchUser()
            {

            }

            public PatchUser(string json)
            {
                JObject jObject = JObject.Parse(json);
                JToken jUser = jObject;
                applicationName = jUser["application_name"].First.ToString();
                name = jUser["Name"].First.ToString();
                email = jUser["Email"].ToString();
                street = jUser["street"].First.ToString();
                municipal = jUser["Municipal"].First.ToString();
                postalCode = jUser["PostalCode"].First.ToString();
                vat = jUser["vat"].First.ToString();               


            }
        
    }
}
