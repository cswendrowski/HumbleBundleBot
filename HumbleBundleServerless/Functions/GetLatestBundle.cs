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
            var couldGetType = int.TryParse(type, out var typeAsInt);

            if (!couldGetType)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, "Could not parse Type " + type);
            }

            var bundleType = (BundleTypes)typeAsInt;

            var bundlesForType = currentTableBundles.Where(x => x.PartitionKey == bundleType.ToString());

            var latest = bundlesForType.ToList().OrderByDescending(x => x.Timestamp).FirstOrDefault();

            if (latest == null)
            {
                req.CreateResponse(HttpStatusCode.NotFound);
            }

            return req.CreateResponse(HttpStatusCode.OK, latest.GetBundle());
        }
    }
}
