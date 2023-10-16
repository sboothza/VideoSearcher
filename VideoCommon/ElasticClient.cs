using MySqlX.XDevAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace VideoCommon
{
    public class ElasticClient
    {
        private readonly string _host;
        private readonly string _username;
        private readonly string _password;
        private readonly string _index;
        private readonly HttpClient _client;

        public ElasticClient(string host, string username, string password, string index)
        {
            _host=host;
            _username=username;
            _password=password;
            _index=index;

            var handler = new HttpClientHandler();
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.ServerCertificateCustomValidationCallback =
                (httpRequestMessage, cert, cetChain, policyErrors) => true;
            _client = new HttpClient(handler);
            _client.BaseAddress = new Uri(_host);
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.ConnectionClose = true;
            var authenticationString = $"{_username}:{_password}";
            var base64EncodedAuthenticationString = Convert.ToBase64String(Encoding.UTF8.GetBytes(authenticationString));
            _client.DefaultRequestHeaders.Add("Authorization", "Basic " + base64EncodedAuthenticationString);
        }

        public bool Test()
        {
            var response = _client.GetAsync(_index).Result;
            return response.IsSuccessStatusCode;
        }

        public List<T> Search<T>(string query) where T:IdObject
        {
            var result = new List<T>();
            try
            {
                var response = _client.PostAsync($"{_index}/_search",
                    new StringContent(query, Encoding.UTF8, "application/json")).Result;
                
                if (response.IsSuccessStatusCode)
                {
                    var document = JsonNode.Parse(response.Content.ReadAsStringAsync().Result);
                    var count = document["hits"]["total"]["value"].ToJsonString();
                    foreach (var node in document["hits"]["hits"].AsArray())
                    {
                        var id = node["_id"].ToString();
                        Console.WriteLine(id);
                        var obj = System.Text.Json.JsonSerializer.Deserialize<T>(node["_source"]);
                        obj.id = long.Parse(id);
                        result.Add(obj);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return result;
        }
    }
}
