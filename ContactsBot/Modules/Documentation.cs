using Discord.Commands;
using System;
using System.Net;
using System.Linq;
using System.Xml.Linq;
using System.Threading.Tasks;
using System.Xml;
using AngleSharp;
using AngleSharp.Parser.Html;

namespace ContactsBot.Modules
{
    public class Documentation : ModuleBase
    {
        private string GetRssUrl(string query) => $"https://social.msdn.microsoft.com/search/en-US/feed?query={Uri.EscapeDataString(query)}&format=RSS";
        private string GetHtmlUrl(string query) => $"https://social.msdn.microsoft.com/Search/en-US?query={Uri.EscapeDataString(query)}";

        private const string RefBaseUrl = "https://referencesource.microsoft.com";
        private string GetRefSourceUrl(string query) => $"{RefBaseUrl}/api/symbols/?symbol={Uri.EscapeDataString(query)}";
        private string GetRefHtmlUrl(string query) => $"{RefBaseUrl}/#q={Uri.EscapeDataString(query)}";

        [Command("docs"), Alias("msdn")]
        [Summary("Searches official Microsoft documentation for a given query.")]
        public async Task MsdnAsync([Summary("The query to search for")] [Remainder] string query)
        {
            try
            {
                var request = HttpWebRequest.CreateHttp(GetRssUrl(query));

                var xmlDoc = XDocument.Load((await request.GetResponseAsync()).GetResponseStream());

                var item = xmlDoc.Descendants(XName.Get("item", ""))
                    .Take(1)
                    .Select(d => new QueryResponse
                    {
                        OriginalQuery = query,
                        Title = d.Descendants(XName.Get("title", "")).FirstOrDefault()?.Value,
                        Description = d.Descendants(XName.Get("description", "")).FirstOrDefault()?.Value,
                        Url = d.Descendants(XName.Get("link", "")).FirstOrDefault()?.Value,
                        ListingUrl = GetHtmlUrl(query)
                    })
                    .FirstOrDefault();

                if (item == null)
                {
                    await ReplyAsync($"No results for **{query}**.");
                    return;
                }

                await ReplyWithResponseAsync(item);
            }
            catch (WebException)
            {
                await ReplyAsync("Sorry, there was a network issue.");
            }
            catch (XmlException)
            {
                await ReplyAsync("There was a problem parsing the XML response from MSDN.");
            }
        }

        [Command("ref"), Alias("source")]
        [Summary("Searches the .NET reference source.")]
        public async Task ReferenceSourceAsync([Summary("The query to search for")] [Remainder] string query)
        {
            //Stupid hack because 1.0 lowercases command args
            query = Context.Message.Content.Substring(Context.Message.Content.IndexOf(' ') + 1);

            try
            {
                var html = await BrowsingContext.New(Configuration.Default.WithDefaultLoader()).OpenAsync(GetRefSourceUrl(query));

                string note = html.QuerySelector("div.note").TextContent;

                var allItems = html.QuerySelectorAll("a");
                var item = allItems.Skip(1).First();

                if (Char.IsUpper(query[0]))
                {
                    var found = allItems.FirstOrDefault(d => d.QuerySelector(".resultKind")?.TextContent == "class" ||
                                                             d.QuerySelector(".resultKind")?.TextContent == "struct");

                    if (found != null)
                    {
                        item = found;
                    }
                }

                if (note == "No results found")
                {
                    await ReplyAsync($"No results for **{query}**.");
                    return;
                }

                await ReplyWithResponseAsync(new QueryResponse
                {
                    OriginalQuery = query
                },
                new Discord.EmbedBuilder
                {
                    Title = item.ParentElement.Id,
                    Url = $"{RefBaseUrl}{item.Attributes["href"].Value}",
                    Footer = new Discord.EmbedFooterBuilder
                    {
                        Text = $"{item.QuerySelector(".resultKind").TextContent} {item.QuerySelector(".resultName").TextContent}",
                        IconUrl = $"{RefBaseUrl}{item.QuerySelector("img").Attributes["src"].Value}"
                    },
                    Color = new Discord.Color(104, 33, 122)
                });
            }
            catch (WebException)
            {
                await ReplyAsync("Sorry, there was a network issue.");
            }
            catch (HtmlParseException)
            {
                await ReplyAsync("There was a problem parsing the HTML response from the reference source.");
            }
        }

        private async Task ReplyWithResponseAsync(QueryResponse response, Discord.EmbedBuilder overload = null)
        {
            Discord.EmbedBuilder embed = overload;

            if (overload == null)
            {
                embed = new Discord.EmbedBuilder
                {
                    Title = response.Title,
                    Description = WebUtility.HtmlDecode(response.Description),
                    Url = response.Url,
                    Color = new Discord.Color(104, 33, 122),
                    Author = new Discord.EmbedAuthorBuilder
                    {
                        Name = $"{new Uri(response.Url).Host} (click for more)",
                        Url = response.ListingUrl
                    }
                };
            }

            await ReplyAsync($"Found something for **{response.OriginalQuery}**!", false, embed);
        }

        private class QueryResponse
        {
            public string OriginalQuery { get; set; }

            public string Title { get; set; }
            public string Description { get; set; }
            public string Url { get; set; }

            public string ListingUrl { get; set; }
        }
    }
}
