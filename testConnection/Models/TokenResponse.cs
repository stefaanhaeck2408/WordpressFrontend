using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace testConnection.Models
{
    class TokenResponse
    {
        public string token { get; set; }

        public TokenResponse()
        {

        }

        public TokenResponse(string json)
        {
            JObject jObject = JObject.Parse(json);
            JToken jUser = jObject;
            token = jUser["token"].ToString();
        }
    }
}
