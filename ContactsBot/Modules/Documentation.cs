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
    [Group("docs")]
    public class Documentation : ModuleBase
    {
        private const string MSDNIcon = "http://i.imgur.com/LCr4b6P.png";
        private const string MSDocsIcon = "http://i.imgur.com/GSXfzSU.jpg";

        private enum ReplyType
        {
            MSDN,
            MSDocs
        }

        [Command("msdn")]
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
                        Url = d.Descendants(XName.Get("link", "")).FirstOrDefault()?.Value,
                        ReplyType = ReplyType.MSDN
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

        [Command("msdocs")]
        public async Task Docs([Summary("The query to search for")] [Remainder] string query)
        {
            try
            {
                var request = HttpWebRequest.CreateHttp($"https://docs.microsoft.com/api/search?search={Uri.EscapeDataString(query)}&locale=en-us&$top=10");
                using (StreamReader reader = new StreamReader((await request.GetResponseAsync()).GetResponseStream()))
                {
                    var response = JObject.Parse(reader.ReadToEnd());
                    var result = response.First.First.First();

                    var item = new QueryResponse
                    {
                        OriginalQuery = query,
                        Title = result["title"].Value<string>(),
                        Description = result["description"].Value<string>(),
                        Url = result["url"].Value<string>(),
                        ReplyType = ReplyType.MSDocs
                    };

                    await ReplyWithResponse(item);
                }
            }
            catch (WebException)
            {
                await ReplyAsync("Sorry, there was a network issue.");
            }
            catch (JsonException)
            {
                await ReplyAsync("There was a problem parsing the JSON response from MSDocs.");
            }
            catch (IOException)
            {
                await ReplyAsync("There was an issue in reading the response from MSDocs.");
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
                Thumbnail = new Discord.EmbedThumbnailBuilder()
                {
                    Url = (response.ReplyType == ReplyType.MSDN ? MSDNIcon : MSDocsIcon)
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

            public ReplyType ReplyType { get; set; }
        }
    }
}
