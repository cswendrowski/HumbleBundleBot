using System;
using HtmlAgilityPack;
using HumbleBundleServerless.Models;
using Newtonsoft.Json;
using ScrapySharp.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Web;

namespace HumbleBundleBot
{
    public class HumbleScraper
    {

        private readonly List<string> _visitedUrls = new List<string>();

        private readonly List<HumbleBundle> _foundBundles = new List<HumbleBundle>();

        public string BaseUrl { get; set; }

        private const string HumbleBundleUrl = "https://www.humblebundle.com/";

        private List<dynamic> BundlesTab = new List<dynamic>();
        private List<string> BundleUrls = new List<string>();

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
            url = HttpUtility.UrlDecode(url);
            url = url.TrimEnd('?');
            if (!url.StartsWith("https://")) url = "https://" + url;
            var document = web.Load(url);
            var response = document.DocumentNode;

            var finalUrl = GetMetaValueByOgProperty(response, "url");

            if (!BundleUrls.Any())
            {
                BundleUrls = GetBundleUrlsFromRss().Distinct().Where(x => x != "https://www.humblebundle.com/subscription").ToList();
            }

            _visitedUrls.Add(url);
            _visitedUrls.Add(finalUrl);

            if (url == BaseUrl)
            {
                VisitOtherPages(BundleUrls);
            }
            else
            {
                try
                {
                    var bundle = new HumbleBundle
                    {
                        Name = GetMetaValueByOgProperty(response, "title"),
                        Description = GetMetaValueByOgProperty(response, "description"),
                        ImageUrl = GetMetaValueByOgProperty(response, "image"),
                        URL = url,
                        Type = GetBundleType(url)
                    };

                    if (string.IsNullOrEmpty(bundle.ImageUrl))
                    {
                        bundle.ImageUrl = GetMetaValueByName(response, "twitter:image");
                    }

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

        private static string GetMetaValueByOgProperty(HtmlNode response, string property)
        {
            return GetMetaValueByProperty(response, "og:" + property);
        }

        private static string GetMetaValueByProperty(HtmlNode response, string property)
        {
            try
            {
                return response.CssSelect("meta").Where(x => x.Attributes.HasKeyIgnoreCase("property")).First(x => x.Attributes["property"].Value == property).Attributes["content"].Value;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Could not retrieve meta with property " + property);
                Debug.WriteLine(e.Message);
                return "";
            }
        }

        private static string GetMetaValueByName(HtmlNode response, string name)
        {
            try
            {
                return response.CssSelect("meta").Where(x => x.Attributes.HasKeyIgnoreCase("name")).First(x => x.Attributes["name"].Value == name).Attributes["content"].Value;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Could not retrieve meta with name " + name);
                Debug.WriteLine(e.Message);
                return "";
            }
        }

        /**
         * Note: No longer used, left for reference
         * The bundles tab is injected via JS after page load. The data it injects is already in-page, however, so we can
         * parse and deserialize it to get the data we want.
         **/
        //private static IEnumerable<dynamic> GetBundlesTab(HtmlNode response)
        //{
        //    const string startString = "\"mosaic\":";
        //    const string endString = "\"user\": {}";

        //    var jsonResponse = response.InnerHtml.Substring(response.InnerHtml.IndexOf(startString, StringComparison.Ordinal) + startString.Length);

        //    var endIndex = jsonResponse.IndexOf(endString, StringComparison.Ordinal);
        //    jsonResponse = jsonResponse.Substring(0, endIndex - "\r\n      ".Length);

        //    jsonResponse = jsonResponse.Replace("True", "true").Replace("False", "false");

        //    var settings = new JsonSerializerSettings
        //    {
        //        NullValueHandling = NullValueHandling.Ignore,
        //        MissingMemberHandling = MissingMemberHandling.Ignore
        //    };

        //    var converted = JsonConvert.DeserializeObject<List<dynamic>>(jsonResponse, settings);

        //    return converted[0]["products"];
        //}

        private static IEnumerable<string> GetBundleUrlsFromRss()
        {
            var reader = XmlReader.Create("http://blog.humblebundle.com/rss");
            var feed = SyndicationFeed.Load(reader);
            reader.Close();

            var lookbackTime = DateTime.UtcNow.AddDays(-21);

            foreach (var item in feed.Items.Where(x => x.PublishDate >= lookbackTime))
            {
                var categories = item.Categories.Select(x => x.Name).Distinct();
                var summary = item.Summary.Text;

                if (categories.Contains("humble trove") || categories.Contains("humble store"))
                {
                    continue;
                }

                var start = summary.IndexOf("https://blog.humblebundle.com");

                if (start == -1)
                {
                    continue;
                } 

                var end = summary.IndexOf("\" class=\"more-link\"");
                var blogUrl = summary.Substring(start, end - start);

                yield return ScrapeBlogPost(blogUrl);
            }
        }

        private static string ScrapeBlogPost(string blogUrl)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(blogUrl);
            httpWebRequest.Timeout = (int) TimeSpan.FromMinutes(1).TotalMilliseconds;
            httpWebRequest.ServerCertificateValidationCallback = AllowAll;
            using (HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                if (httpWebResponse.StatusCode == HttpStatusCode.OK)
                {
                    using (var responseStream = httpWebResponse.GetResponseStream())
                    {
                        using (var reader = new StreamReader(responseStream))
                        {
                            var htmlstring = reader.ReadToEnd();
                            HtmlDocument document = new HtmlDocument();
                            document.LoadHtml(htmlstring);

                            var response = document.DocumentNode;

                            var learnMoreButton = response.CssSelect(".wp-block-button__link").First();
                            var badHref = learnMoreButton.Attributes["href"].Value;

                            if (badHref == "https://www.humblebundle.com/monthly") return badHref;

                            var start = badHref.IndexOf("www.humblebundle.com");
                            var end = badHref.IndexOf("utm_source");
                            var goodUrl = badHref.Substring(start, end - start);

                            return goodUrl;
                        }
                    }

                }
                else
                {
                    throw new Exception("Could not get blog url");
                }
            }
        }

        private static bool AllowAll(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        private static BundleTypes GetBundleType(string url)
        {
            if (url.Contains("/games/") || url.Contains("/monthly"))
            {
                return BundleTypes.GAMES;
            }
            if (url.Contains("/mobile/"))
            {
                return BundleTypes.MOBILE;
            }
            if (url.Contains("/books/"))
            {
                return BundleTypes.BOOKS;
            }
            if (url.Contains("/software/"))
            {
                return BundleTypes.SOFTWARE;
            }
            return BundleTypes.SPECIAL;
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
                var lockedContentChildren = itemTitle.CssSelect(".sr-only");
                foreach (var child in lockedContentChildren)
                {
                    itemTitle.RemoveChild(child);
                }
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

        //private void VisitOtherPages(IEnumerable<dynamic> bundlesTab)
        //{
        //    foreach (var tab in bundlesTab)
        //    {
        //        string nextPage = HumbleBundleUrl.Replace(".com/", ".com") + tab.product_url;

        //        if (!_visitedUrls.Contains(nextPage) && !nextPage.Contains("store"))
        //        {
        //            ScrapePage(nextPage);
        //        }
        //    }
        //}  

        private void VisitOtherPages(IEnumerable<string> bundleUrls)
        {
            foreach (var url in bundleUrls)
            {
                if (!_visitedUrls.Contains(url) && !url.Contains("store"))
                {
                    ScrapePage(url);
                }
            }
        }
    }
}
