using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using HumbleBundleBot;
using RestSharp;

namespace HumbleBundleServerless
{
    public static class FireDiscordMessagesOnQueue
    {
        [FunctionName("FireDiscordMessagesOnQueue")]
        public static void Run([QueueTrigger("discordmessagequeue")]DiscordMessage message, TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed. Firing message to {message.WebhookUrl} : {message.Payload.content}");

            var apiUrl = "https://discordapp.com/api";
            var client = new RestClient(apiUrl);

            var request = new RestRequest(message.WebhookUrl.Replace(apiUrl, ""), Method.POST);

            request.AddHeader("Content-Type", "application/json");
            request.AddJsonBody(message.Payload);

            var response = client.Execute(request);

            log.Info($"Fired message. Got response {response.StatusCode}");
        }
    }
}
