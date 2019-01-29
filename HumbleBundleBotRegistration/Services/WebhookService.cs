using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using HumbleBundleBotRegistration.Models;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace HumbleBundleBotRegistration.Services
{
    public class WebhookService : IWebhookService
    {

        private HttpClient _client;

        public WebhookService()
        {
            if (_client == null)
            {
                _client = new HttpClient();
            }            
        }

        public async Task<bool> RegisterWebhook(RegistrationWebhook registrationWebhook)
        {
            var request = new HttpRequestMessage(HttpMethod.Post,
                "https://humblebundlenotifications.azurewebsites.net/api/RegisterWebhook")
            {
                Content = PrepareWebhookInfo(registrationWebhook)
            };

            var response = await _client.SendAsync(request);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeregisterWebhook(DeregistrationWebhook deregistrationWebhook)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete,
                "https://humblebundlenotifications.azurewebsites.net/api/DeleteWebhook")
            {
                Content = PrepareWebhookInfo(deregistrationWebhook)
            };

            var response = await _client.SendAsync(request);

            return response.IsSuccessStatusCode;
        }

        private StringContent PrepareWebhookInfo<T>(T registrationWebhook)
        {
            var serializerSettings = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore, 
            };
            
            return new StringContent(JsonConvert.SerializeObject(registrationWebhook, serializerSettings), Encoding.UTF8, "application/json");
        }

    }
}
