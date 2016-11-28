using Discord.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Linq;
using System.Xml.Linq;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;

namespace ContactsBot.Modules
{
    public class Documentation : ModuleBase
    {
        private string GetRssUrl(string query) => $"https://social.msdn.microsoft.com/search/en-US/feed?query={Uri.EscapeDataString(query)}&format=RSS";
        private string GetHtmlUrl(string query) => $"https://social.msdn.microsoft.com/Search/en-US?query={Uri.EscapeDataString(query)}";

        [Command("docs")]
        [Alias("msdn")]
        public async Task Msdn([Summary("The query to search for")] [Remainder] string query)
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
                        Url = d.Descendants(XName.Get("link", "")).FirstOrDefault()?.Value
                    })
                    .FirstOrDefault();

                if (item == null)
                {
                    await ReplyAsync($"No results for **{query}**.");
                    return;
                }

                await ReplyWithResponse(item);
            }
            catch (WebException)
            {
                await ReplyAsync("Sorry, there was a network issue.");
            }
            catch (XmlException)
            {
                await ReplyAsync("There was a problem parsing the XML response from MSDN.");
            }
            catch (Exception ex)
            {
                await ReplyAsync($"Something weird happened. {ex.Message}");
            }
        }

        private async Task ReplyWithResponse(QueryResponse response)
        {
            var embed = new Discord.EmbedBuilder
            {
                Title = response.Title,
                Description = WebUtility.HtmlDecode(response.Description),
                Url = response.Url,
                Color = new Discord.Color(104, 33, 122),
                Author = new Discord.EmbedAuthorBuilder
                {
                    Name = $"{new Uri(response.Url).Host} (click for more)",
                    Url = GetHtmlUrl(response.OriginalQuery)
                }
            };

            await ReplyAsync($"Found something for **{response.OriginalQuery}**!", false, embed);
        }

        private class QueryResponse
        {
            public string OriginalQuery { get; set; }

            public string Title { get; set; }
            public string Description { get; set; }
            public string Url { get; set; }
        }
    }
}
