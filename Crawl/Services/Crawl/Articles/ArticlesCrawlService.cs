using Crawl.Models.Articles;
using Crawl.Models.Matches;
using Crawl.Services.Crawl.Helpers;
using HtmlAgilityPack;
using System.Buffers.Text;

namespace Crawl.Services.Crawl.Articles
{
    public class ArticlesCrawlService
    {
        private static readonly HttpClient _httpClient = new();

        private const string BaseUrl = "https://www.filgoal.com";

        private readonly CrawlHelpersService _helpersService;

        public ArticlesCrawlService(CrawlHelpersService helpersService)
        {
            _helpersService = helpersService;

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
        #region Articles

        public async Task<List<Article>?> GetArticlesAsync()
        {
            try
            {
                var html = await _helpersService.GetHtmlAsync($"{BaseUrl}/articles");

                return _helpersService.ParseArticles(html);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetArticlesAsync Error: {ex.Message}");
                return null;
            }
        }

        public async Task<List<Article>?> GetTeamArticles(string teamName)
        {
            try
            {
                var html = await _helpersService.GetHtmlAsync(
                    $"{BaseUrl}/search/filter?keyword={teamName}"
                );

                return _helpersService.ParseArticles(html);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetTeamArticles Error: {ex.Message}");
                return null;
            }
        }

        public async Task<ArticleContent?> GetArticleContentAsync(string articleUrl)
        {
            try
            {
                var html = await _helpersService.GetHtmlAsync($"{BaseUrl}{articleUrl}");

                var doc = _helpersService.LoadDocument(html);

                var article = new ArticleContent
                {
                    Title = doc.DocumentNode
                        .SelectSingleNode("//div[@class='title']//h1")
                        ?.InnerText
                        .Trim(),

                    Text = doc.DocumentNode
                        .SelectSingleNode("//div[@id='details_content']")
                        ?.InnerText
                        .Trim(),

                    PublishedDate = doc.DocumentNode
                        .SelectSingleNode("//div[@class='title']//p")
                        ?.InnerText
                        .Trim(),

                    Author = doc.DocumentNode
                        .SelectSingleNode("//div[@class='title']//p[2]")
                        ?.InnerText
                        .Trim(),

                    Images = doc.DocumentNode
                        .SelectNodes("//div[@class='details']//img")?
                        .Select(x => _helpersService.FixImageUrl(
                            x.GetAttributeValue("data-src", null)
                        ))
                        .ToArray(),

                    RelatedArticles = new List<RelatedArticle>()
                };

                var relatedNodes = doc.DocumentNode
                    .SelectNodes("//div[@class='ntva_box_list']//a");

                if (relatedNodes != null)
                {
                    foreach (var node in relatedNodes)
                    {
                        var relatedDoc = _helpersService.LoadDocument(node.InnerHtml);

                        article.RelatedArticles.Add(new RelatedArticle
                        {
                            Title = relatedDoc.DocumentNode
                                .SelectSingleNode("//span")
                                ?.InnerText
                                .Trim(),

                            Url = node.GetAttributeValue("href", null),

                            ImageUrl = _helpersService.FixImageUrl(
                                relatedDoc.DocumentNode
                                .SelectSingleNode("//img")
                                ?.GetAttributeValue("data-src", null)
                            )
                        });
                    }
                }

                return article;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetArticleContentAsync Error: {ex.Message}");
                return null;
            }
        }

        #endregion


    }
}
