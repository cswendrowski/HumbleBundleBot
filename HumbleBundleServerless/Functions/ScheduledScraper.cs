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
            [Table("humbleBundles")] IQueryable<HumbleBundleEntity> currentBundles,
            [Table("humbleBundles")] ICollector<HumbleBundleEntity> bundlesTable,
            [Table("humbleBundles")] CloudTable bundleTableClient,
            [Queue("bundlequeue")] ICollector<BundleQueue> bundleQueue,
            [Queue("updatequeue")] ICollector<string> updateQueue,
            TraceWriter log)
        {
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");

            var bundles = currentBundles.ToList().Select(x => x.GetBundle()).ToList();

            ScrapeAndCheckBundles(bundles, bundlesTable, bundleTableClient, bundleQueue, log, new HumbleScraper("https://www.humblebundle.com"), BundleTypes.GAMES);
            ScrapeAndCheckBundles(bundles, bundlesTable, bundleTableClient, bundleQueue, log, new HumbleScraper("https://www.humblebundle.com/books"), BundleTypes.BOOKS);
            ScrapeAndCheckBundles(bundles, bundlesTable, bundleTableClient, bundleQueue, log, new HumbleScraper("https://www.humblebundle.com/mobile"), BundleTypes.MOBILE);
            ScrapeAndCheckBundles(bundles, bundlesTable, bundleTableClient, bundleQueue, log, new HumbleScraper("https://www.humblebundle.com/software"), BundleTypes.SOFTWARE);

            ScrapeAndCheckBundles(bundles, bundlesTable, bundleTableClient, bundleQueue, log, new HumbleScraper("https://www.humblebundle.com/extralife"), BundleTypes.SPECIAL);
        }

        private static void ScrapeAndCheckBundles(List<HumbleBundle> currentBundles, ICollector<HumbleBundleEntity> bundlesTable, CloudTable bundleTableClient, ICollector<BundleQueue> bundleQueue, TraceWriter log, HumbleScraper scraper, BundleTypes type)
        {
            var bundles = scraper.Scrape();

            foreach (var bundle in bundles)
            {
                log.Info($"Found current {type.ToString()} Bundle {bundle.Name} with {bundle.Sections.Sum(x => x.Items.Count)} items");

                if (!currentBundles.Any(x => x.Name == bundle.Name))
                {
                    log.Info($"New bundle, adding to table storage");
                    bundlesTable.Add(new HumbleBundleEntity(type, bundle));
                    bundleQueue.Add(new BundleQueue()
                    {
                        Type = type,
                        Bundle = bundle
                    });
                }
                else
                {
                    var currentBundle = currentBundles.First(x => x.Name == bundle.Name);

                    foreach (var section in bundle.Sections)
                    {
                        foreach (var game in section.Items.ToList())
                        {
                            if (currentBundle.Items.Any(x => x.Name == game.Name))
                            {
                                section.Items.Remove(game);
                            }
                        }
                    }

                    if (bundle.Items.Any())
                    {
                        bundleTableClient.Execute(TableOperation.InsertOrReplace(new HumbleBundleEntity(type, bundle)));

                        bundleQueue.Add(new BundleQueue()
                        {
                            Type = type,
                            Bundle = bundle,
                            IsUpdate = true
                        });
                    }
                }
            }
        }
    }
}
