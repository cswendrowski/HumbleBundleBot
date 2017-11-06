using HtmlAgilityPack;
using HumbleBundleServerless.Models;
using Newtonsoft.Json;
using ScrapySharp.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace HumbleBundleBot
{
    public class HumbleScraper
    {

        private List<string> visitedUrls = new List<string>();

        private List<HumbleBundle> foundBundles = new List<HumbleBundle>();

        public string BaseUrl { get; set; }

        public HumbleScraper(string baseUrl)
        {
            BaseUrl = baseUrl;
        }

        public List<HumbleBundle> Scrape()
        {
            ScrapePage(BaseUrl);

            return foundBundles;
        }

        private void ScrapePage(string url)
        {
            var web = new HtmlWeb();
            var document = web.Load(url);
            var response = document.DocumentNode;

            var finalUrl = response.CssSelect("meta").First(x => x.Attributes[0].Value == "og:url").Attributes["content"].Value;

            var bundlesTab = getBundlesTab(response);

            visitedUrls.Add(url);
            visitedUrls.Add(finalUrl);

            var bundle = new HumbleBundle
            {
                Name = GetBundleName(response),
                Description = GetBundleDescription(response),
                ImageUrl = GetBundleImageUrl(response),
                URL = finalUrl,
                Type = GetBundleType(bundlesTab)
            };

            ScrapeSections(bundle, response, finalUrl);

            foundBundles.Add(bundle);

            VisitOtherPages(bundlesTab);
        }

        /**
         * The bundles tab is injected via JS after page load. The data it injects is already in-page, however, so we can
         * parse and deseralize it to get the data we want.
         **/
        private List<dynamic> getBundlesTab(HtmlNode response)
        {
            var startString = "var productTiles = ";
            var jsonResponse = response.InnerHtml.Substring(response.InnerHtml.IndexOf(startString) + startString.Length);
            var endIndex = jsonResponse.IndexOf(";");
            jsonResponse = jsonResponse.Substring(0, endIndex);

            return JsonConvert.DeserializeObject<List<dynamic>>(jsonResponse);
        }

        private BundleTypes GetBundleType(List<dynamic> bundlesTab)
        {
            var activeBundle = bundlesTab.First(x => x.is_active);

            switch (activeBundle.tile_stamp.Value)
            {
                case "games": return BundleTypes.GAMES;
                case "mobile": return BundleTypes.MOBILE;
                case "books": return BundleTypes.BOOKS;
                case "software": return BundleTypes.SOFTWARE;
                default: return BundleTypes.SPECIAL;
            }
        }

        private string GetBundleName(HtmlNode response)
        {
            return response.CssSelect("meta").First(x => x.Attributes[0].Value == "title").Attributes["content"].Value;
        }

        private string GetBundleDescription(HtmlNode response)
        {
            return response.CssSelect("meta").First(x => x.Attributes[0].Value == "og:description").Attributes["content"].Value;
        }

        private string GetBundleImageUrl(HtmlNode response)
        {
            return response.CssSelect("meta").First(x => x.Attributes[0].Value == "og:image").Attributes["content"].Value;
        }

        private void ScrapeSections(HumbleBundle bundle, HtmlNode response, string finalUrl)
        {

            foreach (var parsedSection in response.CssSelect(".dd-game-row"))
            {
                var sectionTitle = "";

                try
                {
                    sectionTitle = parsedSection.CssSelect(".dd-header-headline").First().InnerText.CleanInnerText();
                }
                catch
                {
                    sectionTitle = parsedSection.CssSelect(".fi-content-header").First().InnerText.CleanInnerText();
                }

                if (sectionTitle.Contains("average"))
                {
                    sectionTitle = "Beat the Average!";
                }

                if (string.IsNullOrEmpty(sectionTitle))
                {
                    continue;
                }

                var sectionToAdd = new HumbleSection()
                {
                    Title = sectionTitle
                };

                FindGamesInSection(finalUrl, parsedSection, sectionToAdd);

                bundle.Sections.Add(sectionToAdd);
            }
        }

        private void FindGamesInSection(string finalUrl, HtmlNode parsedSection, HumbleSection section)
        {
            foreach (var itemTitle in parsedSection.CssSelect(".dd-image-box-caption"))
            {
                var itemName = itemTitle.InnerText.CleanInnerText();
                if (!section.Items.Any(x => x.Name == itemName) && !itemName.StartsWith("More in"))
                {
                    section.Items.Add(new HumbleItem()
                    {
                        Name = itemName
                    });
                }
            }

            if (parsedSection.CssSelect(".fi-content-body").Any())
            {
                var itemName = parsedSection.CssSelect(".fi-content-body").First().InnerText.CleanInnerText();
                if (!section.Items.Any(x => x.Name == itemName) && !itemName.StartsWith("More in"))
                {
                    section.Items.Add(new HumbleItem()
                    {
                        Name = itemName
                    });
                }
            }
        }

        private void VisitOtherPages(List<dynamic> bundlesTab)
        {
            foreach (var tab in bundlesTab)
            {
                var nextPage = "https://www.humblebundle.com/" + tab.url;

                if (!visitedUrls.Contains(nextPage))
                {
                    ScrapePage(nextPage);
                }
            }
        }  
    }
}
