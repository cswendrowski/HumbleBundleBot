using HumbleBundleBot;
using HumbleBundleServerless.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
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
            [Queue("bundlequeue")] ICollector<BundleQueue> bundleQueue,
            [Queue("updatequeue")] ICollector<string> updateQueue,
            TraceWriter log)
        {
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");

            var currentBundleNames = new List<String>();

            foreach (var bundle in currentBundles)
            {
                var fullBundle = bundle.GetBundle();
                log.Info($"Found current bundle {fullBundle.Name}");
                currentBundleNames.Add(fullBundle.Name);
            }

            var gameScraper = new HumbleScraper();

            var foundGames = gameScraper.Scrape();

            var bundles = GetBundlesFromGames(foundGames);

            foreach (var bundle in bundles)
            {
                log.Info($"Found Bundle {bundle.Name} with {bundle.Sections.Sum(x => x.Games.Count)} items");

                if (!currentBundleNames.Any(x => x == bundle.Name))
                {
                    log.Info($"New bundle, adding to table storage");
                    bundlesTable.Add(new HumbleBundleEntity(BundleTypes.GAMES, bundle));
                    bundleQueue.Add(new BundleQueue()
                    {
                        Type = BundleTypes.GAMES,
                        Bundle = bundle
                    });
                }
            }
        }

        private static List<HumbleBundle> GetBundlesFromGames(List<HumbleGame> results)
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
