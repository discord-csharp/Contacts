using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;

namespace Rebase.Services.DotNet
{
    public class Nuget
    {
        private string GetNugetFeedUrl(string query) => $"https://api-v2v3search-0.nuget.org/query?q={Uri.EscapeDataString(query)}";
        private string GetNugetHtmlUrl(string query) => $"https://www.nuget.org/packages/{Uri.EscapeDataString(query)}";
        
        public async Task<List<NugetResult>> PerformNugetSearch(string query)
        {
            var request = HttpWebRequest.CreateHttp(GetNugetFeedUrl(query));

            return JsonConvert.DeserializeObject<NugetResult>(await new StreamReader((await request.GetResponseAsync()).GetResponseStream()).ReadToEndAsync());
        }
    }
}
