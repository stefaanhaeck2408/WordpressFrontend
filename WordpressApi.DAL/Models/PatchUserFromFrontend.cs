using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json.Linq;

namespace WordpressApi.DAL.Models
{
    [XmlRoot(ElementName = "patch_user")]
    public class PatchUserFromFrontend: IXsdValidation
    {
        [XmlIgnoreAttribute]
        public int UserId { get; set; }
        public string application_name { get; set; }
        public string name { get; set; }
        public string uuid { get; set; }
        public string email { get; set; }
        public string street { get; set; }
        public string municipal { get; set; }
        public string postalCode { get; set; }
        public string vat { get; set; }

        public PatchUserFromFrontend()
        {

        }

        public PatchUserFromFrontend(string json)
        {
            JObject jObject = JObject.Parse(json);
            JToken jUser = jObject;
            name = jUser["Name"].ToString();
            email = jUser["Email"].ToString();
            street = jUser["street"].ToString();
            municipal = jUser["Municipal"].ToString();
            postalCode = jUser["PostalCode"].ToString();
            vat = jUser["vat"].ToString();
            UserId = (int)jUser["UserId"];
        }
    }
}
