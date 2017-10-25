using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumbleBundleBot
{
    public class HumbleService
    {
        HumbleScraper scraper;
        String filename;
        AzureTableService tableService;

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public HumbleService(HumbleScraper scraper, String filename)
        {
            this.scraper = scraper;
            this.filename = filename;

            tableService = new AzureTableService();
            BundleNames = tableService.GetBundlesForKey(filename.Replace(".jsonl", ""));

            BundleNames.ForEach(x => Console.WriteLine("Loaded: " + x));
        }

        public List<String> BundleNames { get; set; }

        public List<HumbleBundle> GetNewBundles()
        {
            var toReturn = new List<HumbleBundle>();

            var bundles = GetKnownBundles();

            foreach (var bundle in bundles)
            {
                if (!BundleNames.Contains(bundle.Name))
                {
                    toReturn.Add(bundle);
                    BundleNames.Add(bundle.Name);
                    tableService.AddBundleToKey(filename.Replace(".jsonl", ""), bundle.Name);
                }
            }

            return toReturn;
        }

        public List<HumbleBundle> GetKnownBundles()
        {
            var results = scraper.Scrape();

            var toReturn = new List<HumbleBundle>();

            foreach (var bundleResult in results.GroupBy(x => x.Bundle))
            {
                var bundle = new HumbleBundle
                {
                    Name = bundleResult.Key,
                    URL = bundleResult.ToList()[0].URL
                };

                foreach (var section in bundleResult.GroupBy(x => x.Section))
                {
                    bundle.Sections.Add(new HumbleSection()
                    {
                        Title = section.Key,
                        Games = section.ToList().Select(x => x.Title).ToList()
                    });
                }

                toReturn.Add(bundle);
            }

            return toReturn;
        }
    }

    public class HumbleBundle {
        public String Name { get; set; }
        public String URL { get; set; }
        public List<HumbleSection> Sections { get; set; } = new List<HumbleSection>();
    }

    public class HumbleSection
    {
        public String Title { get; set; }
        public List<String> Games { get; set; } = new List<String>();
    }
}
