using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HumbleBundleServerless.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;

namespace HumbleBundleServerless.Functions
{
    public static class GetMetrics
    {
        [FunctionName("GetMetrics")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestMessage req,
            [Table("webhookRegistration")] CloudTable existingWebhooks,
            TraceWriter log)
        {
            var response = new List<Metric>();

            foreach (BundleTypes type in Enum.GetValues(typeof(BundleTypes)))
            {
                var noUpdate = existingWebhooks.GetAllWebhooksForBundleType(type, false);
                response.Add(new Metric { Type = type.ToString(), IsUpdate = false, Count = noUpdate.Count()});

                var update = existingWebhooks.GetAllWebhooksForBundleType(type, true);
                response.Add(new Metric { Type = type.ToString(), IsUpdate = true, Count = update.Count() });
            }

            return req.CreateResponse(HttpStatusCode.OK, response);
        }
    }

    class Metric
    {
        public string Type { get; set; }

        public bool IsUpdate { get; set; }

        public int Count { get; set; }
    }
}
