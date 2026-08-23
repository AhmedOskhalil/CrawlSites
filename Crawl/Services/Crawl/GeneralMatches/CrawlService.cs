using Crawl.Models;
using Crawl.Models.Articles;
using Crawl.Models.Matches;
using Crawl.Models.Vidoes;
using Crawl.Services.Crawl.Helpers;
using HtmlAgilityPack;

namespace Crawl.Services.Crawl.GeneralMatches
{
    public class CrawlService
    {
        private static readonly HttpClient _httpClient = new();

        private const string BaseUrl = "https://www.filgoal.com";

        private readonly CrawlHelpersService _helpersService;

        public CrawlService( CrawlHelpersService helpersService)
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

        #region Matches

        public async Task<List<Partition>> RunSearchDayAsync(string date)
        {
            try
            {
                var html = await _helpersService.GetHtmlAsync($"{BaseUrl}/matches/?date={date}");

                var doc = _helpersService.LoadDocument(html);

                var matchBlocks = doc.DocumentNode
                    .SelectNodes("//div[@class='mc-block']");

                var partitions = new List<Partition>();

                if (matchBlocks == null)
                    return partitions;

                foreach (var block in matchBlocks.Skip(1))
                {
                    var blockDoc =  _helpersService.LoadDocument(block.InnerHtml);

                    var partition = new Partition
                    {
                        PartitionName = blockDoc.DocumentNode
                            .SelectSingleNode("//h6")
                            ?.InnerText
                            .Trim(),

                        Matchs = _helpersService.ParseMatches(
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
                var html = await _helpersService.GetHtmlAsync($"{BaseUrl}/teams/{teamId}/matches-results");

                var doc = _helpersService.LoadDocument(html);

                var teamMatches = new TeamMatches
                {
                    TeamName = doc.DocumentNode
                        .SelectSingleNode("//h1")
                        ?.InnerText
                        .Trim(),

                    TeamLogo = doc.DocumentNode
                        .SelectSingleNode("//h1/a/img")
                        ?.GetAttributeValue("src", null),

                    Matches = _helpersService.ParseMatches(
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
                var html = await _helpersService.GetHtmlAsync($"{BaseUrl}/teams/{teamId}/matches-fixtures");

                var doc = _helpersService.LoadDocument(html);

                var matches = _helpersService.ParseMatches(
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
                var html = await _helpersService.GetHtmlAsync($"{BaseUrl}/teams/{teamId}");

                var doc = _helpersService.LoadDocument(html);

                var team = new TeamInformation
                {
                    teamId = teamId,
                    TeamName = doc.DocumentNode
                        .SelectSingleNode("//h1")
                        ?.InnerText
                        ?.Trim(),

                    TeamLogo = _helpersService.FixImageUrl(
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
                        var matchDoc = _helpersService.LoadDocument(node.InnerHtml);

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

                            HomeTeamLogo = _helpersService.FixImageUrl(
                                matchDoc.DocumentNode
                                .SelectSingleNode("//div[@class='s']//img")
                                ?.GetAttributeValue("data-src", null)
                            ),

                            AwayTeamLogo = _helpersService.FixImageUrl(
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

       


       


    }
}