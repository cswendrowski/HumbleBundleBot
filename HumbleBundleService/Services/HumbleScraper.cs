using HtmlAgilityPack;
using NLog;
using ScrapySharp.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumbleBundleBot
{
    public class HumbleScraper
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        private List<string> visitedUrls = new List<string>();

        private List<HumbleGame> foundGames = new List<HumbleGame>();

        public virtual string GetBaseUrl()
        {
            return "https://www.humblebundle.com";
        }

        public List<HumbleGame> Scrape()
        {
            ScrapePage(GetBaseUrl());

            return foundGames;
        }

        private void ScrapePage(string url)
        {
            var web = new HtmlWeb();
            var document = web.Load(url);
            var response = document.DocumentNode;

            var finalUrl = web.ResponseUri.ToString();

            visitedUrls.Add(finalUrl);

            logger.Info("Scraping "  + finalUrl);

            if (finalUrl != GetBaseUrl())
            {
                var bundleName = response.CssSelect("#active-subtab").First().InnerText.CleanInnerText();

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

                    foreach (var gameTitle in section.CssSelect(".dd-image-box-caption"))
                    {
                        var title = gameTitle.InnerText.CleanInnerText();
                        foundGames.Add(new HumbleGame
                        {
                            Bundle = bundleName,
                            URL = finalUrl,
                            Title = title,
                            Section = sectionTitle
                        });
                    }

                    if (section.CssSelect(".fi-content-body").Any())
                    {
                        var title = section.CssSelect(".fi-content-body").First().InnerText.CleanInnerText();
                        foundGames.Add(new HumbleGame
                        {
                            Bundle = bundleName,
                            URL = finalUrl,
                            Title = title,
                            Section = sectionTitle
                        });
                    }
                }
            }

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

    public class HumbleGame
    {
        public string Bundle { get; set; }
        public string URL { get; set; }
        public string Section { get; set; }
        public string Title { get; set; }

        public override string ToString()
        {
            return "[" + Bundle + "]  [" + Section + "]  " + Title;
        }
    }

    public static class StringExtensions
    {
        public static String Clean(this string text)
        {
            return text.Replace("\n", "").Trim();
        }
    }
}
