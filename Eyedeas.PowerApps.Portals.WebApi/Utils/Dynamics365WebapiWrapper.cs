using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Eyedeas.PowerApps.Portals.WebApi.Utils
{
    public class Dynamics365WebapiWrapper
    {

        string webAPIBaseAddress, resourceURI, authority, clientId, clientSecret, redirectUrl;

        public Dynamics365WebapiWrapper(string _webAPIBaseAddress, string _resourceURI, string _authority,
            string _clientId, string _clientSecret, string _redirectUrl)
        {
            this.webAPIBaseAddress = _webAPIBaseAddress;
            this.resourceURI = _resourceURI;
            this.authority = _authority;
            this.clientId = _clientId;
            this.clientSecret = _clientSecret;
            this.redirectUrl = _redirectUrl;
        }

        public HttpResponseMessage Get(string statement)
        {
            if (string.IsNullOrEmpty(statement)) throw new ArgumentNullException("Statement cannot be blank or null");

            return SendRetrieveRequestAsync(statement, true);

            //if (response.IsSuccessStatusCode)
            //    return Ok(response.Content.ReadAsStringAsync().Result);
            //else
            //    return StatusCode((int)response.StatusCode,
            //        $"Failed to Retrieve records: {response.ReasonPhrase}");
        }

        public HttpResponseMessage Post(string statement, string body)
        {
            return SendCreateRequestAsync(statement, body);
        }

        public HttpResponseMessage Patch(string statement, string body)
        {
            return SendUpdateRequestAsync(statement, body);
        }

        public HttpResponseMessage Delete(string statement)
        {
            return SendDeleteRequestAsync(statement);
        }

        private HttpResponseMessage SendRetrieveRequestAsync(string query, Boolean formatted = false, int maxPageSize = 50)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, query);
            request.Headers.Add("Prefer", "odata.maxpagesize=" + maxPageSize.ToString());
            if (formatted)
                request.Headers.Add("Prefer",
                    "odata.include-annotations=OData.Community.Display.V1.FormattedValue");
            return getHttpClient().SendAsync(request).Result;
        }

        private HttpResponseMessage SendCreateRequestAsync(string endPoint, string content)
        {
            return SendAsync(HttpMethod.Post, endPoint, content);
        }

        private HttpResponseMessage SendUpdateRequestAsync(string endPoint, string content)
        {
            var patch = new HttpMethod("PATCH");
            return SendAsync(patch, endPoint, content);
        }

        private HttpResponseMessage SendAsync(HttpMethod operation, string endPoint, string content)
        {
            HttpRequestMessage request = new HttpRequestMessage(operation, endPoint);
            request.Content = new StringContent(content.ToString(), Encoding.UTF8, "application/json");
            return getHttpClient().SendAsync(request).Result;
        }

        public HttpResponseMessage SendDeleteRequestAsync(string endPoint)
        {
            return getHttpClient().DeleteAsync(endPoint).Result;
        }

        private HttpClient getHttpClient()
        {
            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(webAPIBaseAddress);
            httpClient.Timeout = new TimeSpan(0, 2, 0);  // 2 minutes  
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AcquireToken(resourceURI, authority, clientId, 
                clientSecret, redirectUrl));

            return httpClient;
        }

        private string AcquireToken(string resourceURI, string authority, string clientId, string clientSecret, string redirectUrl)
        {
            HttpClient client = new HttpClient();
            var postData = $"client_id={clientId}&client_secret={System.Uri.EscapeDataString(clientSecret)}&resource={resourceURI}&grant_type=client_credentials";
            string loginUrl = $"https://login.microsoftonline.com/{authority}/oauth2/token";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, loginUrl);
            request.Content = new StringContent(postData, Encoding.UTF8);
            request.Content.Headers.Remove("Content-Type");
            request.Content.Headers.TryAddWithoutValidation("Content-Type", $"application/x-www-form-urlencoded");

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var responseMessage = client.SendAsync(request).Result;
            var jsonResponseString = responseMessage.Content.ReadAsStringAsync().Result;
            
            var jsonContent = JSonUtils.Deserialize<AzureAppAuthentication>(jsonResponseString);
            return jsonContent.access_token;
        }

    }
}
