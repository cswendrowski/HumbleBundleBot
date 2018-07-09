using HumbleBundleBot;
using HumbleBundleServerless.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HumbleBundleServerless
{
    public static class ScheduledScraper
    {
        [FunctionName("ScheduledScraper")]
        public static void Run([TimerTrigger("0 0 */4 * * *")]TimerInfo myTimer,
            [Table("humbleBundles")] IQueryable<HumbleBundleEntity> currentTableBundles,
            [Table("humbleBundles")] ICollector<HumbleBundleEntity> bundlesTable,
            [Table("humbleBundles")] CloudTable bundleTableClient,
            [Queue("bundlequeue")] ICollector<BundleQueue> bundleQueue,
            [Queue("jsonbundlequeue")] ICollector<BundleQueue> jsonMessageQueue,
            TraceWriter log)
        {
            log.Info($"Scraper function executed at: {DateTime.Now}");

            var currentBundles = currentTableBundles.ToList().Select(x => x.GetBundle()).ToList();

            var bundles = new HumbleScraper("https://www.humblebundle.com").Scrape();

            foreach (var bundle in bundles)
            {
                log.Info($"Found current {bundle.Type.ToString()} Bundle {bundle.Name} with {bundle.Sections.Sum(x => x.Items.Count)} items");

                if (currentBundles.All(x => x.Name != bundle.Name))
                {
                    log.Info($"New bundle, adding to table storage");
                    bundlesTable.Add(new HumbleBundleEntity(bundle));

                    AddToQueues(bundleQueue, jsonMessageQueue, bundle);
                }
                else
                {
                    var currentBundle = currentBundles.First(x => x.Name == bundle.Name);

                    var foundItems = bundle.Sections.SelectMany(x => x.Items).ToList();


                    foreach (var item in foundItems.ToList())
                    {
                        if (currentBundle.Items.Any(x => x.Name == item.Name))
                        {
                            foundItems.Remove(item);
                        }
                    }


                    if (foundItems.Any())
                    {
                        bundleTableClient.Execute(TableOperation.InsertOrReplace(new HumbleBundleEntity(bundle)));

                        AddToQueues(bundleQueue, jsonMessageQueue, bundle);
                    }
                }
            }
        }

        private static void AddToQueues(ICollector<BundleQueue> bundleQueue, ICollector<BundleQueue> jsonMessageQueue, HumbleBundle bundle)
        {
            bundleQueue.Add(new BundleQueue()
            {
                Bundle = bundle,
                IsUpdate = true
            });

            jsonMessageQueue.Add(new BundleQueue()
            {
                Bundle = bundle,
                IsUpdate = true
            });
        }
    }
}
