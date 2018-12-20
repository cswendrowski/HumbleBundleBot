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
    public static class GetLatestBundles
    {
        [FunctionName("GetLatestBundles")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "LatestBundles/{type}/{cnt}")] HttpRequestMessage req,
            string type,
            int cnt,
            [Table("humbleBundles")] IQueryable<HumbleBundleEntity> currentTableBundles,
            TraceWriter log)
        {
            var couldGetType = int.TryParse(type, out var typeAsInt);

            if (!couldGetType)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, "Could not parse Type " + type);
            }

            var bundleType = (BundleTypes)typeAsInt;

            var bundlesForType = currentTableBundles.Where(x => x.PartitionKey == bundleType.ToString()).ToList();

            if (!bundlesForType.Any())
            {
                req.CreateResponse(HttpStatusCode.NotFound);
            }

            var latest = bundlesForType.OrderByDescending(x => x.Timestamp).Take(cnt).Select(x => new BundleQueue() { Bundle = x.GetBundle(), IsUpdate = false } );

            return req.CreateResponse(HttpStatusCode.OK, latest);
        }
    }
}
