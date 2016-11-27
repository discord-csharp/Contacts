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
        private const string MSDNIcon = "http://i.imgur.com/LCr4b6P.png";

        [Command("docs")]
        public async Task Msdn([Summary("The query to search for")] [Remainder] string query)
        {
            try
            {
                var request = HttpWebRequest.CreateHttp($"https://social.msdn.microsoft.com/search/en-US/feed?query={Uri.EscapeDataString(query)}&format=RSS");

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
            catch (Exception)
            {
                await ReplyAsync("Something weird happened.");
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
                Thumbnail = new Discord.EmbedThumbnailBuilder
                {
                    Url = MSDNIcon
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
