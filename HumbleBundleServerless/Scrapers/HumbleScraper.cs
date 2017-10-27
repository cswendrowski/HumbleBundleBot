using HtmlAgilityPack;
using ScrapySharp.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace HumbleBundleBot
{
    public class HumbleScraper
    {

        private List<string> visitedUrls = new List<string>();

        private List<HumbleItem> foundGames = new List<HumbleItem>();

        public string BaseUrl { get; set; }

        public HumbleScraper(string baseUrl)
        {
            BaseUrl = baseUrl;
        }

        public List<HumbleItem> Scrape()
        {
            ScrapePage(BaseUrl);

            return foundGames;
        }

        private void ScrapePage(string url)
        {
            var web = new HtmlWeb();
            var document = web.Load(url);
            var response = document.DocumentNode;

            var finalUrl = response.CssSelect("meta").First(x => x.Attributes[0].Value == "og:url").Attributes["content"].Value;

            visitedUrls.Add(finalUrl);

            if (finalUrl != BaseUrl)
                ScrapeSections(response, finalUrl);

            VisitOtherPages(response);
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

        private void ScrapeSections(HtmlNode response, string finalUrl)
        {
            var bundleName = GetBundleName(response);
            var bundleDescrption = GetBundleDescription(response);
            var bundleImageUrl = GetBundleImageUrl(response);

            foreach (var section in response.CssSelect(".dd-game-row"))
            {
                var sectionTitle = "";

                try
                {
                    sectionTitle = section.CssSelect(".dd-header-headline").First().InnerText.CleanInnerText();
                }
                catch
                {
                    sectionTitle = section.CssSelect(".fi-content-header").First().InnerText.CleanInnerText();
                }

                if (sectionTitle.Contains("average"))
                {
                    sectionTitle = "Beat the Average!";
                }

                if (string.IsNullOrEmpty(sectionTitle))
                {
                    continue;
                }

                FindGamesInSection(finalUrl, bundleName, bundleDescrption, bundleImageUrl, section, sectionTitle);
            }
        }

        private void FindGamesInSection(string finalUrl, string bundleName, string bundleDescription, string bundleImageUrl, HtmlNode section, string sectionTitle)
        {
            foreach (var gameTitle in section.CssSelect(".dd-image-box-caption"))
            {
                var title = gameTitle.InnerText.CleanInnerText();
                if (!foundGames.Any(x => x.Title == title))
                {
                    foundGames.Add(new HumbleItem
                    {
                        Bundle = bundleName,
                        BundleDescription = bundleDescription,
                        BundleImageUrl = bundleImageUrl,
                        URL = finalUrl,
                        Title = title,
                        Section = sectionTitle
                    });
                }
            }

            if (section.CssSelect(".fi-content-body").Any())
            {
                var title = section.CssSelect(".fi-content-body").First().InnerText.CleanInnerText();
                if (!foundGames.Any(x => x.Title == title))
                {
                    foundGames.Add(new HumbleItem
                    {
                        Bundle = bundleName,
                        BundleDescription = bundleDescription,
                        BundleImageUrl = bundleImageUrl,
                        URL = finalUrl,
                        Title = title,
                        Section = sectionTitle
                    });
                }
            }
        }

        private void VisitOtherPages(HtmlNode response)
        {
            foreach (var tab in response.CssSelect(".subtab-button").Where(x => x.GetAttributeValue("href").StartsWith("/")))
            {
                var nextPage = "https://www.humblebundle.com" + tab.Attributes["href"].Value;

                if (!visitedUrls.Contains(nextPage))
                {
                    ScrapePage(nextPage);
                }
            }
        }  
    }

    public class HumbleItem
    {
        public string Bundle { get; set; }
        public string BundleDescription { get; set; }
        public string BundleImageUrl { get; set; }
        public string URL { get; set; }
        public string Section { get; set; }
        public string Title { get; set; }

        public override string ToString()
        {
            return "[" + Bundle + "]  [" + Section + "]  " + Title;
        }
    }
}
