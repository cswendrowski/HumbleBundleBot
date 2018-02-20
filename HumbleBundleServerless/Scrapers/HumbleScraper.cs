using System;
using HtmlAgilityPack;
using HumbleBundleServerless.Models;
using Newtonsoft.Json;
using ScrapySharp.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace HumbleBundleBot
{
    public class HumbleScraper
    {

        private readonly List<string> _visitedUrls = new List<string>();

        private readonly List<HumbleBundle> _foundBundles = new List<HumbleBundle>();

        public string BaseUrl { get; set; }

        private const string HumbleBundleUrl = "https://www.humblebundle.com/";

        public HumbleScraper(string baseUrl)
        {
            BaseUrl = baseUrl;
        }

        public List<HumbleBundle> Scrape()
        {
            ScrapePage(BaseUrl);

            return _foundBundles;
        }

        private void ScrapePage(string url)
        {
            var web = new HtmlWeb();
            var document = web.Load(url);
            var response = document.DocumentNode;

            var finalUrl = GetOgPropertyValue(response, "url");

            var bundlesTab = GetBundlesTab(response);

            _visitedUrls.Add(url);
            _visitedUrls.Add(finalUrl);

            if (url == BaseUrl)
            {
                VisitOtherPages(bundlesTab);
            }
            else
            {
                try
                {
                    var bundle = new HumbleBundle
                    {
                        Name = GetOgPropertyValue(response, "title"),
                        Description = GetOgPropertyValue(response, "description"),
                        ImageUrl = GetOgPropertyValue(response, "image"),
                        URL = finalUrl,
                        Type = GetBundleType(bundlesTab)
                    };

                    ScrapeSections(bundle, response);

                    _foundBundles.Add(bundle);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                    // Do nothing
                }
            }
            
        }

        private static string GetOgPropertyValue(HtmlNode response, string property)
        {
            return response.CssSelect("meta").Where(x => x.Attributes.HasKeyIgnoreCase("property")).First(x => x.Attributes["property"].Value == "og:" + property).Attributes["content"].Value;
        }

        /**
         * The bundles tab is injected via JS after page load. The data it injects is already in-page, however, so we can
         * parse and deseralize it to get the data we want.
         **/
        private static List<dynamic> GetBundlesTab(HtmlNode response)
        {
            const string startString = "var productTiles = ";
            var jsonResponse = response.InnerHtml.Substring(response.InnerHtml.IndexOf(startString, StringComparison.Ordinal) + startString.Length);
            var endIndex = jsonResponse.IndexOf(";", StringComparison.Ordinal);
            jsonResponse = jsonResponse.Substring(0, endIndex);

            jsonResponse = jsonResponse.Replace("True", "true").Replace("False", "false");

            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            return JsonConvert.DeserializeObject<List<dynamic>>(jsonResponse, settings);
        }

        private static BundleTypes GetBundleType(IEnumerable<dynamic> bundlesTab)
        {
            try
            {
                var activeBundle = bundlesTab.First(x => (bool) x.is_active);

                switch (activeBundle.tile_stamp.Value)
                {
                    case "games": return BundleTypes.GAMES;
                    case "mobile": return BundleTypes.MOBILE;
                    case "books": return BundleTypes.BOOKS;
                    case "comics": return BundleTypes.BOOKS;
                    case "software": return BundleTypes.SOFTWARE;
                    default: return BundleTypes.SPECIAL;
                }
            }
            catch (Exception e)
            {
                throw new Exception("Could not determine active bundle type due to: " + e.Message);
            }
        }

        private static void ScrapeSections(HumbleBundle bundle, HtmlNode response)
        {

            foreach (var parsedSection in response.CssSelect(".dd-game-row"))
            {
                string sectionTitle;

                try
                {
                    try
                    {
                        sectionTitle = parsedSection.CssSelect(".dd-header-headline").First().InnerText
                            .CleanInnerText();
                    }
                    catch
                    {
                        sectionTitle = parsedSection.CssSelect(".fi-content-header").First().InnerText.CleanInnerText();
                    }
                }
                catch (Exception)
                {
                    sectionTitle = string.Empty;
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

                FindGamesInSection(parsedSection, sectionToAdd);

                bundle.Sections.Add(sectionToAdd);
            }
        }

        private static void FindGamesInSection(HtmlNode parsedSection, HumbleSection section)
        {
            foreach (var itemTitle in parsedSection.CssSelect(".dd-image-box-caption"))
            {
                var itemName = itemTitle.InnerText.CleanInnerText();
                if (section.Items.All(x => x.Name != itemName) && !itemName.StartsWith("More in"))
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
                if (section.Items.All(x => x.Name != itemName) && !itemName.StartsWith("More in"))
                {
                    section.Items.Add(new HumbleItem()
                    {
                        Name = itemName
                    });
                }
            }
        }

        private void VisitOtherPages(IEnumerable<dynamic> bundlesTab)
        {
            foreach (var tab in bundlesTab)
            {
                var nextPage = HumbleBundleUrl + tab.url;

                if (!_visitedUrls.Contains(nextPage))
                {
                    ScrapePage(nextPage);
                }
            }
        }  
    }
}
