using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Xml.Serialization;
using Newtonsoft.Json;
using RabbitMqReceiver.Models;
using WordPressPCL;
using WordPressPCL.Client;
using WordPressPCL.Models;

namespace RabbitMqReceiver.Services
{
    public class Services
    {
        public async System.Threading.Tasks.Task<IEnumerable<WordPressPCL.Models.User>> TestUsersConnectionAsync() {
            //Connect to wordpress
            var client = new WordPressClient("http://127.0.0.1/wordpress/wp-json");
            client.AuthMethod = WordPressPCL.Models.AuthMethod.JWT;
            await client.RequestJWToken("stefaan.haeck@student.ehb.be", "integration");

            //Get user with userId retrieved from uuid
            var users = await client.Users.GetAll();
            if (users.Count() > 0)
            {
                return users;
            }
            else {
                return null;
            }
            
        }



        public async System.Threading.Tasks.Task<WordPressPCL.Models.User> ReceivingPatchUserAsync(string xml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(PatchUser));

            PatchUser receivedUser;

            using (StringReader reader = new StringReader(xml))
            {
                receivedUser = (PatchUser)serializer.Deserialize(reader);

                //Make request for get /uuids/uuid
                string responseUuid = null;
                string url = "http://192.168.1.2/uuid-master/uuids/" + receivedUser.uuid;
                using (var httpClient = new HttpClient())
                using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                {

                    using (var responseUuidHttpClient = await httpClient
                        .SendAsync(request, HttpCompletionOption.ResponseHeadersRead)
                        .ConfigureAwait(false))
                    {
                        //responseUuidHttpClient.EnsureSuccessStatusCode();
                        responseUuid = await responseUuidHttpClient.Content.ReadAsStringAsync();


                    }
                }

                //Convert the response from json to dictionary
                var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseUuid);


                //Connect to wordpress
                var client = new WordPressClient("http://127.0.0.1/wordpress/wp-json");
                client.AuthMethod = WordPressPCL.Models.AuthMethod.JWT;
                await client.RequestJWToken("stefaan.haeck@student.ehb.be", "integration");

                //Get user with userId retrieved from uuid
                var users = await client.Users.GetAll();

                //Select user with matching id
                var user = users.Where(x => x.Id == int.Parse(values["frontend"])).FirstOrDefault();

                //make meta fields
                Dictionary<string, string> metadata = new Dictionary<string, string>();

                if (receivedUser.street != null)
                {
                    metadata.Add("street", receivedUser.street);
                }
                if (receivedUser.name != null)
                {
                    metadata.Add("name", receivedUser.name);
                }
                if (receivedUser.municipal != null)
                {
                    metadata.Add("municipal", receivedUser.municipal);
                }
                if (receivedUser.postalCode != null)
                {
                    metadata.Add("postal_code", receivedUser.postalCode);
                }
                if (receivedUser.vat != null)
                {
                    metadata.Add("vat", receivedUser.vat);
                }

                //Update user values
                if (receivedUser.email != "")
                {
                    user.UserName = receivedUser.email;
                    user.Email = receivedUser.email;
                    user.Meta = metadata;
                }
                var response = await client.Users.Update(user);

                return response;
            }
        }
    }
}
