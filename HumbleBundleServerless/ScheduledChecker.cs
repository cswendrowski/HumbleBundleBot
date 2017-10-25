using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using RestSharp;

namespace HumbleBundleServerless
{
    public static class ScheduledChecker
    {
        [FunctionName("ScheduledChecker")]
        public static void Run([TimerTrigger(" 0 0 12 1/1 * ? *")]TimerInfo myTimer, TraceWriter log)
        {
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");

            var client = new RestClient("http://humblebundleservice.azurewebsites.net/checker");

            var request = new RestRequest(Method.POST);

            client.Execute(request);
        }
    }
}
