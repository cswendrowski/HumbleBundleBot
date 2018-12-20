using HumbleBundleServerless.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace HumbleBundleServerless.Functions
{
    public static class GetLatestBundle
    {
        [FunctionName("GetLatestBundle")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "LatestBundle/{type}")] HttpRequestMessage req,
            string type,
            [Table("humbleBundles")] IQueryable<HumbleBundleEntity> currentTableBundles,
            TraceWriter log)
        {
            var latest = currentTableBundles.OrderByDescending(x => x.Timestamp).FirstOrDefault(x => x.PartitionKey == type);

            if (latest == null)
            {
                req.CreateResponse(HttpStatusCode.NotFound);
            }

            return req.CreateResponse(HttpStatusCode.OK, latest);
        }
    }
}
