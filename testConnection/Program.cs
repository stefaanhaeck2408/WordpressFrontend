using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using testConnection.Models;
using WordPressPCL;
using WordPressPCL.Models;

namespace testConnection
{
    class Program
    {
        static async Task Main(string[] args)
        {
            /*string responseBodyNonce = null;
            var nonce = new Dictionary<string, string>();
            var auth = new Dictionary<string, string>();
            //niet vergeten userid generic te maken
            var client = new HttpClient();
            using (var request = new HttpRequestMessage(HttpMethod.Post, "http://127.0.0.1/wordpress/api/get_nonce/?controller=user&method=generate_auth_cookie")) {

                using (var response = await client
                    .SendAsync(request, HttpCompletionOption.ResponseHeadersRead)
                    .ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();
                    responseBodyNonce = await response.Content.ReadAsStringAsync();
                    nonce = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseBodyNonce);
                }
            }
            Dictionary<string, object> test = new Dictionary<string, object>();
           using (var request = new HttpRequestMessage(HttpMethod.Post, "http://127.0.0.1/wordpress/api/user/generate_auth_cookie/?username=stefaan.haeck@student.ehb.be&password=integration&insecure=cool"))
            {

                using (var response = await client
                    .SendAsync(request, HttpCompletionOption.ResponseHeadersRead)
                    .ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();
                    test = JsonConvert.DeserializeObject<Dictionary<string, object>>(body);
                }
            }

            var url = "http://127.0.0.1/wordpress/api/user/register/?username=john&email=john@domain.com&display_name=John&notify=both&seconds=100&insecure=cool";
            var bodyCall = "";
           using (var request = new HttpRequestMessage(HttpMethod.Post, url))
            {
                HttpContent stringContent = new StringContent(nonce["nonce"]);
                MultipartFormDataContent formData = new MultipartFormDataContent();
                formData.Add(stringContent, "nonce");
                request.Content = formData;
                using (var response = await client
                    .SendAsync(request, HttpCompletionOption.ResponseHeadersRead)
                    .ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();
                    bodyCall = await response.Content.ReadAsStringAsync();                    
                }
            }*/
            /*var client = new HttpClient();
            using (var request = new HttpRequestMessage(HttpMethod.Post, "http://127.0.0.1/wordpress/wp-json/jwt-auth/v1/token"))
            {

                using (var response = await client
                    .SendAsync(request, HttpCompletionOption.ResponseHeadersRead)
                    .ConfigureAwait(false))
                {
                    HttpContent stringContent = new StringContent(nonce["nonce"]);
                    MultipartFormDataContent formData = new MultipartFormDataContent();
                    formData.Add(stringContent, "nonce");
                    request.Content = formData;
                    using (var response = await client
                        .SendAsync(request, HttpCompletionOption.ResponseHeadersRead)
                        .ConfigureAwait(false))
                    {
                        response.EnsureSuccessStatusCode();
                        bodyCall = await response.Content.ReadAsStringAsync();
                    }
                }
            }*/


            var client = new WordPressClient("http://127.0.0.1/wordpress/wp-json");
            client.AuthMethod = WordPressPCL.Models.AuthMethod.JWT;
            await client.RequestJWToken("stefaan.haeck@student.ehb.be", "integration");

            Dictionary<string, string> metadata = new Dictionary<string, string>();

            metadata.Add("street", "nijverheidskaai 177");
            metadata.Add("name", "lavar");






            if (await client.IsValidJWToken())
            {
                var user = new User
                {
                    UserName = "lavar18",
                    Email = "lavar18@ball.be",
                    Password = "tester123456",
                    Meta = metadata
                };


                
                var test = await client.Users.Create(user);
                /* }


                 }
                 catch (Exception ex) {
                     Console.WriteLine("error: " + ex);
                 }

         */


            }





        }
    }
}
