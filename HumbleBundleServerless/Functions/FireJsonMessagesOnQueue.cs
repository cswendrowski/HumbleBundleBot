using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using HumbleBundleBot;
using RestSharp;

namespace HumbleBundleServerless
{
    public static class FireJsonMessagesOnQueue
    {
        [FunctionName("FireJsonMessagesOnQueue")]
        public static void Run([QueueTrigger("jsonmessagequeue")]JsonMessage message, TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed. Firing message to {message.WebhookUrl} : {message.Payload.Bundle}");

            var client = new RestClient(message.WebhookUrl);

            var request = new RestRequest { Method = Method.POST };

            request.AddHeader("Content-Type", "application/json");
            request.AddJsonBody(message.Payload);

            var response = client.Execute(request);

            log.Info($"Fired message. Got response {response.StatusCode}");
        }
    }
}
