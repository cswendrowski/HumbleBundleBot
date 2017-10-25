using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using System.Configuration;
using Microsoft.WindowsAzure.Storage.Table;

namespace HumbleBundleBot
{
    public class AzureTableService
    {
        CloudStorageAccount storageAccount;

        public AzureTableService()
        {
            storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
        }

        private CloudTableClient GetClient()
        {
            return storageAccount.CreateCloudTableClient();
        }

        private CloudTable GetTable(String name)
        {
            var client = GetClient();

            var table = client.GetTableReference(name);

            table.CreateIfNotExists();

            return table;
        }

        public List<string> GetBundlesForKey(string key)
        {
            Console.WriteLine("Loading from " + key);

            var entity = GetEntity(key);

            if (entity != null)
            {
                Console.WriteLine("Loaded, found "  + entity.Bundles);
                return entity.BundleList;
            }
            else
            {
                Console.WriteLine("Not Found");
                return new List<string>();
            }
        }

        private HumbleEntity GetEntity(string key)
        {
            var table = GetTable("humbleBundleBot");

            var result = table.Execute(TableOperation.Retrieve<HumbleEntity>(key, key)).Result;

            return ((HumbleEntity)result);
        }

        public void AddBundleToKey(string key, string bundleName)
        {
            Console.WriteLine("Adding " + bundleName + " to " + key);

            var entity = GetEntity(key);
            var table = GetTable("humbleBundleBot");

            if (entity == null)
            {
                entity = new HumbleEntity(key);
                var list = entity.BundleList;
                list.Add(bundleName);
                entity.Bundles = string.Join(",", list);
                table.Execute(TableOperation.Insert(entity));
                Console.WriteLine("Inserted");
            }
            else
            {
                var list = entity.BundleList;
                list.Add(bundleName);
                entity.Bundles = string.Join(",", list);
                table.Execute(TableOperation.Replace(entity));
                Console.WriteLine("Replaced");
            }
        }
    }

    public class HumbleEntity : TableEntity
    {
        public HumbleEntity(string keyName)
        {
            PartitionKey = keyName;
            RowKey = keyName;
        }

        public HumbleEntity() { }

        public List<string> BundleList
        {
            get
            {
                if (string.IsNullOrEmpty(Bundles))
                {
                    return new List<string>();
                }
                return Bundles.Split(',').ToList();
            }
        }

        public string Bundles { get; set; }

    }
}
