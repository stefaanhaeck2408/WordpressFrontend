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
            string responseBodyNonce = null;
            
            //niet vergeten userid generic te maken
            var client = new HttpClient();
            using (var request = new HttpRequestMessage(HttpMethod.Post, "http://127.0.0.1/wordpress/wp-json/wp/v2/users?username=lavar_zijn_vrouw7&email=lavarzijnvrouw7@ball.be&password=tester123456")) {

                using (var response = await client
                    .SendAsync(request, HttpCompletionOption.ResponseHeadersRead)
                    .ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();
                    responseBodyNonce = await response.Content.ReadAsStringAsync();
                    nonce = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseBodyNonce);
                }
            }
            

            





        }
    }
}
