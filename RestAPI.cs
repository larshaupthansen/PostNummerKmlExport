using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace PostNummerKlmExport
{
    public class RestAPI
    {

        public async Task<T> Get<T>(string url, Action doneCallback = null)
        {


            var client = new WebClient();

            client.Headers.Add("Content-Type", "application/json");
            //client.Headers.Add("Authorization", "Bearer " + authenticationToken.Token);

            var response = client.DownloadDataTaskAsync(url);

            if (doneCallback != null)
            {
                client.DownloadDataCompleted += (s, e) => { doneCallback(); };
            }

            var stream = new MemoryStream(await response);
            var jsonSerializer = new DataContractJsonSerializer(typeof(T));

            object objResponse = jsonSerializer.ReadObject(stream);

            return (T)objResponse;
        }

        public async Task<dynamic> Get(string url)
        {


            var client = new WebClient();

            client.Headers.Add("Content-Type", "application/json");
            //client.Headers.Add("Authorization", "Bearer " + authenticationToken.Token);

            var response = await client.DownloadStringTaskAsync(url);

            dynamic d = JObject.Parse(response);
            return d;
        }

        public async Task<bool> Put<T>(string url, T jsonObject)
        {
            var response = await PutPost<T>(url, jsonObject, "PUT");
            return true;
        }

        private async Task<WebClient> PutPost<T>(string url, T jsonObject, string method = "POST")
        {

            var client = new WebClient();

            byte[] array;
            string json = ObjectToJson(jsonObject, out array);


            var response = await client.UploadDataTaskAsync(url, method, array);

            return client;
        }

        private static string ObjectToJson<T>(T jsonObject, out byte[] array)
        {
            var jsonSerializer = new DataContractJsonSerializer(jsonObject.GetType());
            var stream = new MemoryStream();

            jsonSerializer.WriteObject(stream, jsonObject);

            array = stream.GetBuffer();
            return System.Text.Encoding.UTF8.GetString(array);
        }
    }
}
