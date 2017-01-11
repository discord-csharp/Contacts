using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Rebase.Services.Common
{
    public class Github
    {
        private string GetGithubSearchUrl(string query, string language) => $"https://api.github.com/search/repositories?q={Uri.EscapeDataString(query)}+language:csharp&sort=stars&order=desc";

        public async Task<GithubResult> PerformGithubSearch(string query, string language = "csharp")
        {
            var request = WebRequest.CreateHttp(GetGithubSearchUrl(query, language));
            request.Headers[HttpRequestHeader.UserAgent] = "Contacts Bot [discord-csharp/Contacts]";

            WebResponse response = null;

            try
            {
                response = (await request.GetResponseAsync());
            }
            catch (WebException ex)
            {
                string errorResponse = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                return null;
            }

            string jsonResult = await new StreamReader(response.GetResponseStream()).ReadToEndAsync();

            return JsonConvert.DeserializeObject<GithubResult>(jsonResult);
        }
    }
}
