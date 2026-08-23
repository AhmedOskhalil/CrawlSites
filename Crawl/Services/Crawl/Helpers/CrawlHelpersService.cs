using Crawl.Models.Articles;
using Crawl.Models.Matches;
using HtmlAgilityPack;

namespace Crawl.Services.Crawl.Helpers
{
    public class CrawlHelpersService
    {
        private static readonly HttpClient _httpClient = new();

        public CrawlHelpersService()
        {
            if (!_httpClient.DefaultRequestHeaders.Contains("referer"))
            {
                _httpClient.DefaultRequestHeaders.Add("referer", "https://www.google.com/");
            }

            if (!_httpClient.DefaultRequestHeaders.Contains("user-agent"))
            {
                _httpClient.DefaultRequestHeaders.Add(
                    "user-agent",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0 Safari/537.36"
                );
            }
        }

        #region Helpers

        public async Task<string> GetHtmlAsync(string url)
        {
            var response = await _httpClient.GetAsync(url);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public HtmlDocument LoadDocument(string html)
        {
            var doc = new HtmlDocument();

            doc.LoadHtml(html);

            return doc;
        }

        public List<Article> ParseArticles(string html)
        {
            var doc = LoadDocument(html);

            var articleNodes = doc.DocumentNode
                .SelectNodes("//main//li");

            var articles = new List<Article>();

            if (articleNodes == null)
                return articles;

            foreach (var node in articleNodes)
            {
                var articleDoc = LoadDocument(node.InnerHtml);

                articles.Add(new Article
                {
                    Title = articleDoc.DocumentNode
                        .SelectSingleNode("//h6")
                        ?.InnerText
                        .Trim(),

                    Url = articleDoc.DocumentNode
                        .SelectSingleNode("//a")
                        ?.GetAttributeValue("href", null),

                    imageUrl = FixImageUrl(
                        articleDoc.DocumentNode
                        .SelectSingleNode("//img")
                        ?.GetAttributeValue("data-src", null)
                    )
                });
            }

            return articles;
        }

        public List<Match> ParseMatches(HtmlNodeCollection? matchNodes)
        {
            var matches = new List<Match>();

            if (matchNodes == null)
                return matches;

            foreach (var node in matchNodes)
            {
                var doc = LoadDocument(node.InnerHtml);

                matches.Add(new Match
                {
                    HomeTeamName = doc.DocumentNode
                        .SelectSingleNode("//div[@class='s']//strong")
                        ?.InnerText
                        .Trim(),

                    AwayTeamName = doc.DocumentNode
                        .SelectSingleNode("//div[@class='f']//strong")
                        ?.InnerText
                        .Trim(),

                    HomeScore = doc.DocumentNode
                        .SelectSingleNode("//div[@class='s']//b")
                        ?.InnerText
                        .Trim(),

                    AwayScore = doc.DocumentNode
                        .SelectSingleNode("//div[@class='f']//b")
                        ?.InnerText
                        .Trim(),

                    Date = GetMatchDate(doc),

                    HomeLogo = FixImageUrl(
                        doc.DocumentNode
                        .SelectSingleNode("//div[@class='s']//img")
                        ?.GetAttributeValue("data-src", null)
                    ),

                    AwayLogo = FixImageUrl(
                        doc.DocumentNode
                        .SelectSingleNode("//div[@class='f']//img")
                        ?.GetAttributeValue("data-src", null)
                    ),

                    Partition = new Partition
                    {
                        PartitionName = doc.DocumentNode
                            .SelectSingleNode("//div[@class='cin_cntnr']//a")
                            ?.InnerText
                            .Trim()
                    }
                });
            }

            return matches;
        }

        public string? GetMatchDate(HtmlDocument doc)
        {
            var span2 = doc.DocumentNode.SelectSingleNode("//span[2]")?.InnerText;
            var span3 = doc.DocumentNode.SelectSingleNode("//span[3]")?.InnerText;
            var span4 = doc.DocumentNode.SelectSingleNode("//span[4]")?.InnerText;

            if (!string.IsNullOrWhiteSpace(span2) && span2.Contains(":"))
                return span2;

            if (!string.IsNullOrWhiteSpace(span3) && span3.Contains(":"))
                return span3;

            return span4;
        }

        public string? FixImageUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            if (url.StartsWith("//"))
                return "https:" + url;

            return url;
        }

        #endregion
    }
}
