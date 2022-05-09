using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DotLiquid.Website.ViewComponents
{
    public class ContributorsViewComponent : ViewComponent
    {
        private readonly HttpClient _httpClient;

        public ContributorsViewComponent()
        {
            _httpClient = new HttpClient(new GitHubHttpClientHandler());
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var contributors = await GetContributorsAsync();
            return View(contributors);
        }

        private async Task<IEnumerable<Contributor>> GetContributorsAsync()
        {
            var contributors = JsonConvert.DeserializeObject<List<Contributor>>(await _httpClient.GetStringAsync("https://api.github.com/repos/dotliquid/dotliquid/contributors"));
            return contributors
                .Where(c => c.Login != "tgjones")
                .OrderByDescending(c => c.Contributions)
                .Take(4)
                .Select(c => JsonConvert.DeserializeObject<Contributor>(_httpClient.GetStringAsync($"https://api.github.com/users/{c.Login}").Result));
        }
    }

    public class Contributor
    {
        public string Login { get; set; }

        [JsonProperty(PropertyName = "html_url")]
        public string HtmlUrl { get; set; }

        public int Contributions { get; set; }

        public string Name { get; set; }

        public string Blog { get; set; }
    }

    public class GitHubHttpClientHandler : HttpClientHandler
    {

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Add("User-Agent", "DotLiquid.Website");
            request.Headers.Add("Accept", "application/vnd.github.v3+json");
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
