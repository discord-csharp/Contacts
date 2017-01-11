using AngleSharp;
using AngleSharp.Parser.Html;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Rebase.Services.CSharp
{
    public class DevNetwork
    {
        private string GetRssUrl(string query) => $"https://social.msdn.microsoft.com/search/en-US/feed?query={Uri.EscapeDataString(query)}&format=RSS";
        private string GetHtmlUrl(string query) => $"https://social.msdn.microsoft.com/Search/en-US?query={Uri.EscapeDataString(query)}";

        private const string RefBaseUrl = "https://referencesource.microsoft.com";
        private string GetRefSourceUrl(string query) => $"{RefBaseUrl}/api/symbols/?symbol={Uri.EscapeDataString(query)}";
        private string GetRefHtmlUrl(string query) => $"{RefBaseUrl}/#q={Uri.EscapeDataString(query)}";

        public async Task<IEnumerable<SearchResult>> ReferenceSource(string query)
        {
            try
            {
                var html = await BrowsingContext.New(Configuration.Default.WithDefaultLoader()).OpenAsync(GetRefSourceUrl(query));

                string note = html.QuerySelector("div.note").TextContent;

                var allItems = html.QuerySelectorAll("a");
                return allItems.Skip(1).Take(3).Select(item => 
                    new SearchResult(item.ParentElement.Id,
                        $"{item.QuerySelector(".resultKind").TextContent} {item.QuerySelector(".resultName").TextContent}",
                        $"{RefBaseUrl}{item.Attributes["href"].Value}",
                        $"{RefBaseUrl}{item.QuerySelector("img").Attributes["src"].Value}"));
            }
            catch (WebException ex)
            {
                throw new RebaseServiceException("There was a problem parsing the XML response from the reference source", ex);
            }
            catch (HtmlParseException ex)
            {
                throw new RebaseServiceException("There was a problem parsing the HTML response from the reference source.", ex);
            }
        }

        public async Task<IEnumerable<SearchResult>> MsdnSearch(string query)
        {
            try
            {
                var request = HttpWebRequest.CreateHttp(GetRssUrl(query));

                var xmlDoc = XDocument.Load((await request.GetResponseAsync()).GetResponseStream());

                return xmlDoc.Descendants(XName.Get("item", ""))
                    .Select(d => new SearchResult
                    (
                        title: d.Descendants(XName.Get("title", "")).FirstOrDefault()?.Value,
                        desc: d.Descendants(XName.Get("description", "")).FirstOrDefault()?.Value,
                        url: d.Descendants(XName.Get("link", "")).FirstOrDefault()?.Value,
                        imageUrl: null
                    ));
            }
            catch (WebException ex)
            {
                throw new RebaseServiceException("There was a network issue!", ex);
            }
            catch (XmlException ex)
            {
                throw new RebaseServiceException("There was a problem parsing the XML response from MSDN", ex);
            }
        }

        public struct SearchResult
        {
            public string Title;
            public string Description;
            public string Url;
            public string ImageUrl;

            public SearchResult(string title, string desc, string url, string imageUrl)
            {
                Title = title;
                Description = desc;
                Url = url;
                ImageUrl = imageUrl;
            }
        }
    }
}
