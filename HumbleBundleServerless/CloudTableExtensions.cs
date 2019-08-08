using HumbleBundleServerless.Models;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;

namespace HumbleBundleServerless
{
    public static class CloudTableExtensions
    {
        public static IEnumerable<WebhookRegistrationEntity> GetAllWebhooksForBundleType(this CloudTable existingWebhooks, BundleTypes type, bool isUpdate)
        {
            var filter = TableQuery.CombineFilters(
                    TypeEquals(type),
                    TableOperators.And,
                    TableQuery.GenerateFilterConditionForInt("WebhookType", QueryComparisons.Equal, (int)WebhookType.Discord)
                );

            if (isUpdate)
            {
                filter = TableQuery.CombineFilters(
                    filter,
                    TableOperators.And,
                    TableQuery.GenerateFilterConditionForBool("ShouldRecieveUpdates", QueryComparisons.Equal, true));
            }

            var query = new TableQuery<WebhookRegistrationEntity>().Where(filter);

            TableContinuationToken token = null;
            do
            {
                // TODO: When we upgrade to C# 8.0, use await
                var queryResult = existingWebhooks.ExecuteQuerySegmentedAsync(query, token).GetAwaiter().GetResult();
                foreach (var item in queryResult.Results)
                {
                    yield return item;
                }
                token = queryResult.ContinuationToken;
            } while (token != null);
        }

        private static string TypeEquals(BundleTypes type)
        {
            return TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, type.ToString()),
                TableOperators.Or,
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, BundleTypes.ALL.ToString())
                );
        }
    }
}
