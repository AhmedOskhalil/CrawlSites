using Crawl.Models;
using Crawl.Models.Articles;
using Crawl.Models.Matches;
using Crawl.Models.Vidoes;
using HtmlAgilityPack;

namespace Crawl.Services.Crawl
{
    public class CrawlService
    {
        private static readonly HttpClient _httpClient = new();

        private const string BaseUrl = "https://www.filgoal.com";

        public CrawlService()
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

        #region Matches

        public async Task<List<Partition>> RunSearchDayAsync(string date)
        {
            try
            {
                var html = await GetHtmlAsync($"{BaseUrl}/matches/?date={date}");

                var doc = LoadDocument(html);

                var matchBlocks = doc.DocumentNode
                    .SelectNodes("//div[@class='mc-block']");

                var partitions = new List<Partition>();

                if (matchBlocks == null)
                    return partitions;

                foreach (var block in matchBlocks.Skip(1))
                {
                    var blockDoc = LoadDocument(block.InnerHtml);

                    var partition = new Partition
                    {
                        PartitionName = blockDoc.DocumentNode
                            .SelectSingleNode("//h6")
                            ?.InnerText
                            .Trim(),

                        Matchs = ParseMatches(
                            blockDoc.DocumentNode.SelectNodes("//div[@class='cin_cntnr']")
                        )
                    };

                    partitions.Add(partition);
                }

                return partitions;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RunSearchDayAsync Error: {ex.Message}");
                return new List<Partition>();
            }
        }

        public async Task<TeamMatches?> GetTeamMatchesAsync(int teamId)
        {
            try
            {
                var html = await GetHtmlAsync($"{BaseUrl}/teams/{teamId}/matches-results");

                var doc = LoadDocument(html);

                var teamMatches = new TeamMatches
                {
                    TeamName = doc.DocumentNode
                        .SelectSingleNode("//h1")
                        ?.InnerText
                        .Trim(),

                    TeamLogo = doc.DocumentNode
                        .SelectSingleNode("//h1/a/img")
                        ?.GetAttributeValue("src", null),

                    Matches = ParseMatches(
                        doc.DocumentNode.SelectNodes("//div[@class='cin_cntnr']")
                    )
                };

                var featureMatches = await GetMatchesFeature(teamId);

                if (featureMatches != null)
                {
                    teamMatches.Matches.AddRange(featureMatches);
                }

                return teamMatches;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetTeamMatchesAsync Error: {ex.Message}");
                return null;
            }
        }

        public async Task<List<Match>?> GetMatchesFeature(int teamId)
        {
            try
            {
                var html = await GetHtmlAsync($"{BaseUrl}/teams/{teamId}/matches-fixtures");

                var doc = LoadDocument(html);

                var matches = ParseMatches(
                    doc.DocumentNode.SelectNodes("//div[@class='cin_cntnr']")
                );

                foreach (var match in matches)
                {
                    match.IsFeatureMatch = true;
                }

                return matches;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetMatchesFeature Error: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Team Information

        public async Task<TeamInformation?> GetTeamInformationAsync(int teamId)
        {
            try
            {
                var html = await GetHtmlAsync($"{BaseUrl}/teams/{teamId}");

                var doc = LoadDocument(html);

                var team = new TeamInformation
                {
                    teamId = teamId,
                    TeamName = doc.DocumentNode
                        .SelectSingleNode("//h1")
                        ?.InnerText
                        ?.Trim(),

                    TeamLogo = FixImageUrl(
                        doc.DocumentNode
                        .SelectSingleNode("//img[contains(@class,'logo')]")
                        ?.GetAttributeValue("src", null)
                    ),

                    RecentMatches = new List<RecentMatch>(),

                    TeamOrder = new TeamOrder
                    {
                        Values = Array.Empty<string>()
                    }
                };

                var orderNodes = doc.DocumentNode
                    .SelectNodes("//div[contains(@data-group-id, '#')]");

                if (orderNodes != null)
                {
                    foreach (var node in orderNodes)
                    {
                        var values = node
                            .SelectSingleNode(".//div[@class='fg_rw s']")
                            ?.SelectNodes(".//div");

                        team.TeamOrder.Values = values?
                            .Select(x => x.InnerText.Trim())
                            .ToArray()
                            ?? Array.Empty<string>();

                        break;
                    }
                }

                var recentMatchNodes = doc.DocumentNode
                    .SelectNodes("//div[@class='cmim']");

                if (recentMatchNodes != null)
                {
                    foreach (var node in recentMatchNodes.Take(2))
                    {
                        var matchDoc = LoadDocument(node.InnerHtml);

                        var teams = matchDoc.DocumentNode
                            .SelectNodes("//div[@class='mims']");

                        if (teams == null || teams.Count < 2)
                            continue;

                        var recentMatch = new RecentMatch
                        {
                            HomeTeamName = team.TeamName,

                            AwayTeamName =
                                teams[0].InnerText.Trim().Split("      ")[^1] != team.TeamName
                                ? teams[0].InnerText.Trim().Split("      ")[^1]
                                : teams[1].InnerText.Trim().Split("      ")[^1],

                            MatchResult =
                                $"{teams[0].InnerText.Trim()[0]} - {teams[1].InnerText.Trim()[0]}",

                            MatchDate = matchDoc.DocumentNode
                                .SelectSingleNode("//span[contains(text(),':')]")
                                ?.InnerText
                                .Trim(),

                            HomeTeamLogo = FixImageUrl(
                                matchDoc.DocumentNode
                                .SelectSingleNode("//div[@class='s']//img")
                                ?.GetAttributeValue("data-src", null)
                            ),

                            AwayTeamLogo = FixImageUrl(
                                matchDoc.DocumentNode
                                .SelectSingleNode("//div[@class='f']//img")
                                ?.GetAttributeValue("data-src", null)
                            )
                        };

                        team.RecentMatches.Add(recentMatch);
                    }
                }

                return team;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetTeamInformationAsync Error: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Articles

        public async Task<List<Article>?> GetArticlesAsync()
        {
            try
            {
                var html = await GetHtmlAsync($"{BaseUrl}/articles");

                return ParseArticles(html);
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
                var html = await GetHtmlAsync(
                    $"{BaseUrl}/search/filter?keyword={teamName}"
                );

                return ParseArticles(html);
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
                var html = await GetHtmlAsync($"{BaseUrl}{articleUrl}");

                var doc = LoadDocument(html);

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
                        .Select(x => FixImageUrl(
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
                        var relatedDoc = LoadDocument(node.InnerHtml);

                        article.RelatedArticles.Add(new RelatedArticle
                        {
                            Title = relatedDoc.DocumentNode
                                .SelectSingleNode("//span")
                                ?.InnerText
                                .Trim(),

                            Url = node.GetAttributeValue("href", null),

                            ImageUrl = FixImageUrl(
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

        #region Helpers

        private async Task<string> GetHtmlAsync(string url)
        {
            var response = await _httpClient.GetAsync(url);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        private HtmlDocument LoadDocument(string html)
        {
            var doc = new HtmlDocument();

            doc.LoadHtml(html);

            return doc;
        }

        private List<Article> ParseArticles(string html)
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

        private List<Match> ParseMatches(HtmlNodeCollection? matchNodes)
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

        private string? GetMatchDate(HtmlDocument doc)
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

        private string? FixImageUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            if (url.StartsWith("//"))
                return "https:" + url;

            return url;
        }

        #endregion
        #region vidoes
        public async Task<List<VideoItem>> GetVideosAsync()
        {
            try
            {
                var html = await GetHtmlAsync("https://www.filgoal.com/videos");

                var doc = LoadDocument(html);

                var videoNodes = doc.DocumentNode
                    .SelectNodes("//div[@class='vfg_item']");

                var videos = new List<VideoItem>();

                if (videoNodes == null)
                    return videos;

                foreach (var node in videoNodes)
                {
                    var videoDoc = LoadDocument(node.InnerHtml);

                    var links = videoDoc.DocumentNode.SelectNodes("//a");

                    var video = new VideoItem
                    {
                        Title = videoDoc.DocumentNode
                            .SelectSingleNode("//span[@itemprop='name']")
                            ?.InnerText
                            ?.Trim() ?? "",

                        Url = links?[0]
                            ?.GetAttributeValue("href", "") ?? "",

                        Thumbnail = FixImageUrl(
                            videoDoc.DocumentNode
                            .SelectSingleNode("//img")
                            ?.GetAttributeValue("data-src", "")
                        ) ?? "",

                        PublishDate = links?[1]
                            ?.SelectNodes(".//span")?[1]
                            ?.InnerText
                            ?.Trim() ?? ""
                    };

                    videos.Add(video);
                }

                return videos;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetVideosAsync Error: {ex.Message}");

                return new List<VideoItem>();
            }
        }

        public async Task<string?> GetVideoUrlAsync(string videoUrl)
        {
            try
            {
                var html = await GetHtmlAsync(videoUrl);

                var doc = LoadDocument(html);

                var iframe = doc.DocumentNode.SelectSingleNode("//div[@class='v_object reel']//iframe");
                var vidframelink = doc.DocumentNode.SelectSingleNode("//div[@id=\"details_content\"]").InnerHtml.ToString().Split("<iframe src=")[1].Split(" ")[0];
                if (iframe == null)
                {
                    if (!string.IsNullOrEmpty(vidframelink))
                        return vidframelink.Replace("\"", "");
                    return null;
                }

                return iframe.GetAttributeValue("src", null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetVideoUrlAsync Error: {ex.Message}");
                return null;
            }
        }
        #endregion


    }
}