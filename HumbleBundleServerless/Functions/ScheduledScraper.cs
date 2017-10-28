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
        public static void Run([TimerTrigger("*/15 * * * * *")]TimerInfo myTimer,
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
        }

        private static void ScrapeAndCheckBundles(List<HumbleBundle> currentBundles, ICollector<HumbleBundleEntity> bundlesTable, CloudTable bundleTableClient, ICollector<BundleQueue> bundleQueue, TraceWriter log, HumbleScraper scraper, BundleTypes type)
        {
            var foundGames = scraper.Scrape();

            var bundles = GetBundlesFromItems(foundGames);

            foreach (var bundle in bundles)
            {
                log.Info($"Found current {type.ToString()} Bundle {bundle.Name} with {bundle.Sections.Sum(x => x.Games.Count)} items");

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
                    var updatedGames = new List<HumbleItem>();

                    var currentBundle = currentBundles.First(x => x.Name == bundle.Name);

                    foreach (var section in bundle.Sections)
                    {
                        var currentSection = currentBundle.Sections.First(x => x.Title == section.Title);

                        foreach (var game in section.Games)
                        {
                            if (!currentSection.Games.Contains(game))
                            {
                                updatedGames.Add(new HumbleItem()
                                {
                                    Title = game,
                                    Bundle = bundle.Name,
                                    BundleDescription = bundle.Description,
                                    BundleImageUrl = bundle.ImageUrl,
                                    Section = section.Title,
                                    URL = bundle.URL
                                });
                            }
                        }
                    }

                    if (updatedGames.Any())
                    {
                        var updatedBundle = GetBundlesFromItems(updatedGames);

                        bundleTableClient.Execute(TableOperation.InsertOrReplace(new HumbleBundleEntity(type, bundle)));

                        bundleQueue.Add(new BundleQueue()
                        {
                            Type = type,
                            Bundle = updatedBundle.First(),
                            IsUpdate = true
                        });
                    }
                }
            }
        }

        private static List<HumbleBundle> GetBundlesFromItems(List<HumbleItem> results)
        {
            var toReturn = new List<HumbleBundle>();

            foreach (var bundleResult in results.GroupBy(x => x.Bundle))
            {
                var bundle = new HumbleBundle
                {
                    Name = bundleResult.Key,
                    URL = bundleResult.First().URL,
                    Description = bundleResult.First().BundleDescription,
                    ImageUrl = bundleResult.First().BundleImageUrl
                };

                foreach (var section in bundleResult.GroupBy(x => x.Section))
                {
                    bundle.Sections.Add(new HumbleSection()
                    {
                        Title = section.Key,
                        Games = section.Select(x => x.Title).ToList()
                    });
                }

                toReturn.Add(bundle);
            }

            return toReturn;
        }
    }
}
