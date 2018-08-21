using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HttpUtils
{
    public class Http
    {
        public enum RequestType
        {
            GET,
            POST,
            PUT,
            DELETE,
        }

        public static T Request<T>(RequestType requestType, string url, object content, JsonSerializerSettings jsonSerializerSettings)
        {
            if ((requestType == RequestType.GET || requestType == RequestType.DELETE) && content != null)
                throw new Exception("The request types GET and DELETE doesn't allow content. To send a content uses the request types POST or PUT");

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.Timeout = TimeSpan.FromSeconds(60);

                HttpResponseMessage response = null;
                StringContent httpContent = null;

                if (content != null)
                {
                    var conteudoJson = JsonConvert.SerializeObject(content, Formatting.Indented, jsonSerializerSettings);
                    httpContent = new StringContent(conteudoJson, Encoding.UTF8, "application/json");
                }

                switch (requestType)
                {
                    case RequestType.GET:
                        response = httpClient.GetAsync(url).Result;
                        break;

                    case RequestType.POST:
                        response = httpClient.PostAsync(url, httpContent).Result;
                        break;

                    case RequestType.PUT:
                        response = httpClient.PutAsync(url, httpContent).Result;
                        break;

                    case RequestType.DELETE:
                        response = httpClient.DeleteAsync(url).Result;
                        break;

                    default:
                        throw new NotImplementedException("Request type not implemented: " + requestType);

                }


                using (response)
                {
                    string responseString = response.Content.ReadAsStringAsync().Result;

                    // (200 = GET, 201 = POST, 202 = POST ou PUT ou DELETE, 204 = PUT ou DELETE)
                    if (response.StatusCode == System.Net.HttpStatusCode.OK || response.StatusCode == System.Net.HttpStatusCode.Created ||
                        response.StatusCode == System.Net.HttpStatusCode.Accepted || response.StatusCode == System.Net.HttpStatusCode.NoContent)
                    {

                        response.EnsureSuccessStatusCode();

                        T result = JsonConvert.DeserializeObject<T>(responseString, jsonSerializerSettings);

                        return result;

                    }
                    // Error
                    else
                        throw new Exception(responseString);

                }

            }

        }
    }
}
