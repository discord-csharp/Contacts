using Discord.Commands;
using System;
using System.Net;
using System.Linq;
using System.Xml.Linq;
using System.Threading.Tasks;
using System.Xml;
using AngleSharp;
using AngleSharp.Parser.Html;
using Humanizer;
using Newtonsoft.Json;
using System.IO;
using NLog;

namespace ContactsBot.Modules.Documentation
{
    public class DocModule : ModuleBase
    {
        private string GetRssUrl(string query) => $"https://social.msdn.microsoft.com/search/en-US/feed?query={Uri.EscapeDataString(query)}&format=RSS";
        private string GetHtmlUrl(string query) => $"https://social.msdn.microsoft.com/Search/en-US?query={Uri.EscapeDataString(query)}";

        private const string RefBaseUrl = "https://referencesource.microsoft.com";
        private string GetRefSourceUrl(string query) => $"{RefBaseUrl}/api/symbols/?symbol={Uri.EscapeDataString(query)}";
        private string GetRefHtmlUrl(string query) => $"{RefBaseUrl}/#q={Uri.EscapeDataString(query)}";

        private string GetNugetFeedUrl(string query) => $"https://api-v2v3search-0.nuget.org/query?q={Uri.EscapeDataString(query)}";
        private string GetNugetHtmlUrl(string query) => $"https://www.nuget.org/packages/{Uri.EscapeDataString(query)}";

        private string GetGithubSearchUrl(string query) => $"https://api.github.com/search/repositories?q={Uri.EscapeDataString(query)}+language:csharp&sort=stars&order=desc";
        private static Logger DocLogger { get; } = LogManager.GetCurrentClassLogger();

        [Command("github"), Alias("gh")]
        [Summary("Searches Github for a repo.")]
        public async Task Github([Summary("The query to search for")] [Remainder] string query)
        {
            var request = HttpWebRequest.CreateHttp(GetGithubSearchUrl(query));
            request.Headers[HttpRequestHeader.UserAgent] = "Contacts Bot [discord-csharp/Contacts]";

            WebResponse response = null;

            try
            {
                response = (await request.GetResponseAsync());
            }
            catch (WebException ex)
            {
                string errorResponse = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                await ReplyAsync($"Network Error. **{ex.Message}** - {errorResponse}");
                return;
            }

            string jsonResult = await new StreamReader(response.GetResponseStream()).ReadToEndAsync();

            var result = JsonConvert.DeserializeObject<GithubResult>(jsonResult).items.FirstOrDefault();

            if (result == null)
            {
                await ReplyAsync($"No results for **{query}**.");
                DocLogger.Info("{0} attempted to query for {1} and no results could be found.", Context.User.Username, query);
                return;
            }

            var embed = new Discord.EmbedBuilder
            {
                Title = result.name,
                Description = $"{result.description}",
                Color = new Discord.Color(157, 224, 240),
                Author = new Discord.EmbedAuthorBuilder
                {
                    Name = result.owner.login,
                    IconUrl = result.owner.avatar_url,
                    Url = result.owner.html_url
                },
                Url = result.html_url
            };

            embed.AddField(x =>
            {
                x.Name = "Stars";
                x.Value = $"{result.stargazers_count:n0}";
                x.IsInline = true;
            });

            embed.AddField(x =>
            {
                x.Name = "Open Issues";
                x.Value = $"{result.open_issues_count:n0}";
                x.IsInline = true;
            });

            embed.AddField(x =>
            {
                x.Name = "Forks";
                x.Value = $"{result.forks_count:n0}";
                x.IsInline = true;
            });

            embed.AddField(x =>
            {
                x.Name = "Watchers";
                x.Value = $"{result.watchers_count:n0}";
                x.IsInline = true;
            });

            await ReplyAsync("", false, embed);
        }

        [Command("nuget"), Alias("nupkg")]
        [Summary("Searches the NuGet repository for a package.")]
        public async Task Nuget([Summary("The query to search for")] [Remainder] string query)
        {
            var request = HttpWebRequest.CreateHttp(GetNugetFeedUrl(query));

            var result = JsonConvert.DeserializeObject<NugetResult>
                (await new StreamReader((await request.GetResponseAsync()).GetResponseStream()).ReadToEndAsync())
                .data.FirstOrDefault();

            if (result == null)
            {
                await ReplyAsync($"No results for **{query}**.");
                DocLogger.Info("{0} attempted to query for {1} and no results could be found.", Context.User.Username, query);
                return;
            }

            var embed = new Discord.EmbedBuilder
            {
                Title = result.title,
                Description = $"{result.description}\n```PM> Install-Package {result.id}```",
                Color = new Discord.Color(104, 33, 122),
                Author = new Discord.EmbedAuthorBuilder
                {
                    Name = String.Join(", ", result.authors),
                    IconUrl = result.iconUrl
                },
                Url = GetNugetHtmlUrl(result.id)
            };

            embed.AddField(x =>
            {
                x.Name = "Downloads";
                x.Value = $"{result.totalDownloads:n0}";
                x.IsInline = true;
            });

            embed.AddField(x =>
            {
                x.Name = "Version";
                x.Value = result.version;
                x.IsInline = true;
            });

            embed.AddField(x =>
            {
                x.Name = "Project";
                x.Value = result.projectUrl;
            });

            embed.AddField(x =>
            {
                x.Name = "License";
                x.Value = result.licenseUrl;
            });

            await ReplyAsync("", false, embed);
        }

        [Command("docs"), Alias("msdn")]
        [Summary("Searches official Microsoft documentation for a given query.")]
        public async Task Msdn([Summary("The query to search for")] [Remainder] string query)
        {
            try
            {
                var request = HttpWebRequest.CreateHttp(GetRssUrl(query));

                var xmlDoc = XDocument.Load((await request.GetResponseAsync()).GetResponseStream());

                var searchResults = xmlDoc.Descendants(XName.Get("item", ""))
                    .Take(3)
                    .Select(d => new SearchResult
                    (
                        title: d.Descendants(XName.Get("title", "")).FirstOrDefault()?.Value,
                        desc: d.Descendants(XName.Get("description", "")).FirstOrDefault()?.Value,
                        url: d.Descendants(XName.Get("link", "")).FirstOrDefault()?.Value
                    ));

                if (!searchResults.Any())
                {
                    await ReplyAsync($"No results for **{query}**.");
                    DocLogger.Info("{0} attempted to query for {1} and no results could be found.", Context.User.Username, query);
                    return;
                }

                var embed = new Discord.EmbedBuilder
                {
                    Title = $"Results for \"{query}\"",
                    Description = $"[View full search results]({GetHtmlUrl(query)})",
                    Color = new Discord.Color(104, 33, 122)
                };

                foreach (var result in searchResults.Where(d=>d.Url.Length < 120))
                {
                    embed.AddField(x =>
                    {
                        char[] badChars = new char[] { '-', '|' };

                        int titleEnd = badChars
                                    .Select(c => result.Title.LastIndexOf(c))
                                    .OrderByDescending(d => d)
                                    .FirstOrDefault(d => d > -1);

                        x.Name = (titleEnd > 0 ? result.Title.Substring(0, titleEnd) : result.Title).Trim(badChars).TrimEnd();

                        x.Value = $"[{result.Description.Truncate(100)}]({EscapeParens(result.Url)}) *{new Uri(result.Url).Host}*";
                    });
                }

                await ReplyAsync("", false, embed);
            }
            catch (WebException ex)
            {
                await ReplyAsync("Sorry, there was a network issue.");
                DocLogger.Debug(ex, "Sorry, there was a network issue.");
            }
            catch (XmlException ex)
            {
                await ReplyAsync("There was a problem parsing the XML response from MSDN.");
                DocLogger.Debug(ex, "There was a problem parsing the XML response from MSDN.");
            }
        }

        [Command("ref"), Alias("source")]
        [Summary("Searches the .NET reference source.")]
        public async Task ReferenceSource([Summary("The query to search for")] [Remainder] string query)
        {
            try
            {
                var html = await BrowsingContext.New(AngleSharp.Configuration.Default.WithDefaultLoader()).OpenAsync(GetRefSourceUrl(query));

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
                    DocLogger.Info("{0} attempted to query for {1} and no results could be found.", Context.User.Username, query);
                    return;
                }

                await ReplyAsync("", false,
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
            catch (WebException ex)
            {
                await ReplyAsync("Sorry, there was a network issue.");
                DocLogger.Debug(ex, "There was a problem parsing the XML response from MSDN.");
            }
            catch (HtmlParseException ex)
            {
                await ReplyAsync("There was a problem parsing the HTML response from the reference source.");
                DocLogger.Debug(ex, "There was a problem parsing the HTML response from the reference source.");
            }
        }

        private static string EscapeParens(string input)
        {
            return input.Replace("(", "%28").Replace(")", "%29");
        }

        struct SearchResult
        {
            public string Title;
            public string Description;
            public string Url;

            public SearchResult(string title, string desc, string url)
            {
                Title = title;
                Description = desc;
                Url = url;
            }
        }
    }
}
