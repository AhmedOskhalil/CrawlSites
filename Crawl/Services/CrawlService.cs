using Crawl.Components.Pages;
using Crawl.Models;
using HtmlAgilityPack;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;



namespace Crawl.Services
{
    public class CrawlService
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public async Task<List<Partition>> RunSearchDayAsync(string date)
        {
            var url = $"https://www.filgoal.com/matches/?date={date}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);

            // Headers

            request.Headers.Add("referer", "https://www.google.com/");
            request.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/143.0.0.0 Safari/537.36");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var html = await response.Content.ReadAsStringAsync();


            //extract elements by parsing the html 
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            List<Partition> partitionMatches = new List<Partition>();
            var nodes = doc.DocumentNode.SelectNodes("//div[@class='mc-block']");
            int i = 0;
            foreach (var node in nodes)
            {
                if (i == 0)
                {
                    i++;
                    continue;
                }

                List<Match> htmlMatches = new List<Match>();
                var PartitionhDoc = new HtmlDocument();
                PartitionhDoc.LoadHtml(node.InnerHtml);
                var partitionName = PartitionhDoc.DocumentNode.SelectSingleNode("//h6")?.InnerText.Trim();
                var partitionMatcheshtml = PartitionhDoc.DocumentNode.SelectNodes("//div[@class='cin_cntnr']");

                foreach (var matchnode in partitionMatcheshtml)
                {
                    var matchDoc = new HtmlDocument();
                    matchDoc.LoadHtml(matchnode.InnerHtml);

#pragma warning disable CS8601 // Possible null reference assignment.
                    htmlMatches.Add(new Match()
                    {
                        HomeTeamName = matchDoc.DocumentNode.SelectSingleNode("//div[@class='s']//strong")?.InnerText.Trim(),

                        AwayTeamName = matchDoc.DocumentNode.SelectSingleNode("//div[@class='f']//strong")?.InnerText.Trim(),

                        HomeScore = matchDoc.DocumentNode.SelectSingleNode("//div[@class='s']//b")?.InnerText.Trim(),

                        Date = (matchDoc.DocumentNode.SelectSingleNode("//span[2]").InnerText.Contains(":") ?
                        matchDoc.DocumentNode.SelectSingleNode("//span[2]").InnerText :
                        (matchDoc.DocumentNode.SelectSingleNode("//span[3]").InnerText.Contains(":") ?
                        matchDoc.DocumentNode.SelectSingleNode("//span[3]").InnerText : matchDoc.DocumentNode.SelectSingleNode("//span[4]").InnerText)),

                        AwayScore = matchDoc.DocumentNode.SelectSingleNode("//div[@class='f']//b")?.InnerText.Trim(),

                        HomeLogo = matchDoc.DocumentNode.SelectSingleNode("//div[@class='s']//img")?.GetAttributeValue("data-src", null) is string h
                                ? "http:" + h
                                : null,

                        AwayLogo = matchDoc.DocumentNode.SelectSingleNode("//div[@class='f']//img")?.GetAttributeValue("data-src", null) is string a
                                ? "http:" + a
                                : null
                    });
#pragma warning restore CS8601 // Possible null reference assignment.
                }
                partitionMatches.Add(new Partition()
                {
                    PartitionName = partitionName,
                    Matchs = htmlMatches
                });
            }



            return partitionMatches;
        }
        public async Task<Models.TeamInformation> GetTeamInformationAsync(int teamId)
        {
            var url = $"https://www.filgoal.com/teams/{teamId}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("referer", "https://www.google.com/");
            request.Headers.Add("user-agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
                "(KHTML, like Gecko) Chrome/120.0 Safari/537.36");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var html = await response.Content.ReadAsStringAsync();

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var team = new Models.TeamInformation();
            team.RecentMatches = new List<RecentMatch>();

            team.TeamName = doc.DocumentNode.SelectSingleNode("//h1")?.InnerText?.Trim();

            var logoNode = doc.DocumentNode
                .SelectSingleNode("//img[contains(@class,'logo')]");

            team.TeamLogo = logoNode?.GetAttributeValue("src", null);

            if (!string.IsNullOrEmpty(team.TeamLogo) && team.TeamLogo.StartsWith("//"))
                team.TeamLogo = "https:" + team.TeamLogo;
            team.TeamOrder ??= new TeamOrder();

            var orderNode = doc.DocumentNode.SelectSingleNode("//div[@data-group-id='#champ1503']");
            var orderHeaderInfo = orderNode?.SelectSingleNode(".//div[@class='fg_rw']")?.SelectNodes(".//div");
            var headers = orderHeaderInfo?.Select(h => h.InnerText.Trim()).ToArray();

            var orderValueInfo = orderNode?.SelectSingleNode(".//div[@class='fg_rw s']")?.SelectNodes(".//div");
            var values = orderValueInfo?.Select(h => h.InnerText.Trim()).ToArray() ?? Array.Empty<string>();
            team.TeamOrder.Headers = headers ?? Array.Empty<string>();
            team.TeamOrder.Values = values;

            var matchNodes = doc.DocumentNode
                .SelectNodes("//div[@class='cmim']");

            if (matchNodes != null)
            {
                foreach (var node in matchNodes.Take(2))
                {
                    var matchDoc = new HtmlDocument();
                    matchDoc.LoadHtml(node.InnerHtml);

                    var match = new RecentMatch
                    {
                        HomeTeamName = team.TeamName,

                        AwayTeamName = matchDoc.DocumentNode.SelectSingleNode("//div[@class='mims']")?.InnerText.Trim().Split("      ")[^1] != team.TeamName ?
                        matchDoc.DocumentNode.SelectSingleNode("//div[@class='mims']")?.InnerText.Trim().Split("      ")[^1] :
                        matchDoc.DocumentNode.SelectNodes("//div[@class='mims']")[1]?.InnerText.Trim().Split("      ")[^1],

                        MatchResult = matchDoc.DocumentNode.SelectSingleNode("//div[@class='mims']").InnerText.Trim()[0]
                            + " - " + matchDoc.DocumentNode.SelectNodes("//div[@class='mims']")[1]?.InnerText.Trim()[0],

                        MatchDate = matchDoc.DocumentNode.SelectSingleNode("//span[contains(text(),':')]")?.InnerText.Trim(),

                        HomeTeamLogo = matchDoc.DocumentNode.SelectSingleNode("//div[@class='s']//img")?.GetAttributeValue("data-src", null),

                        AwayTeamLogo = matchDoc.DocumentNode.SelectSingleNode("//div[@class='f']//img")?.GetAttributeValue("data-src", null)
                    };

                    if (!string.IsNullOrEmpty(match.HomeTeamLogo) && match.HomeTeamLogo.StartsWith("//"))
                        match.HomeTeamLogo = "http:" + match.HomeTeamLogo;

                    if (!string.IsNullOrEmpty(match.AwayTeamLogo) && match.AwayTeamLogo.StartsWith("//"))
                        match.AwayTeamLogo = "http:" + match.AwayTeamLogo;

                    team.RecentMatches.Add(match);
                }
            }
            team.teamId = teamId;
            return team;
        }

        public async Task<Models.TeamMatches> GetTeamMatchesAsync(int teamId)
        {
            try
            {
                var url = $"https://www.filgoal.com/teams/{teamId}/matches-results";
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("referer", "https://www.google.com/");
                request.Headers.Add("user-agent",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0 Safari/537.36");
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var html = await response.Content.ReadAsStringAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                var TeamMatches = new Models.TeamMatches();
                TeamMatches.TeamName = doc.DocumentNode.SelectSingleNode("//h1")?.InnerText?.Trim();
                TeamMatches.TeamLogo = doc.DocumentNode.SelectSingleNode("//h1/a/img")?.GetAttributeValue("src", null);
                var matchNodes = doc.DocumentNode.SelectNodes("//div[@class='cin_cntnr']");
                var teamMatches = new List<Match>();
                foreach (var matchNode in matchNodes)
                {
                    var match = new Models.Match();
                    var matchDoc = new HtmlDocument();
                    matchDoc.LoadHtml(matchNode.InnerHtml);
                    teamMatches.Add(new Match()
                    {
                        HomeTeamName = matchDoc.DocumentNode.SelectSingleNode("//div[@class='s']//strong")?.InnerText.Trim(),

                        AwayTeamName = matchDoc.DocumentNode.SelectSingleNode("//div[@class='f']//strong")?.InnerText.Trim(),

                        HomeScore = matchDoc.DocumentNode.SelectSingleNode("//div[@class='s']//b")?.InnerText.Trim(),

                        Date = (matchDoc.DocumentNode.SelectSingleNode("//span[2]").InnerText.Contains(":") ?
                         matchDoc.DocumentNode.SelectSingleNode("//span[2]").InnerText :
                         (matchDoc.DocumentNode.SelectSingleNode("//span[3]").InnerText.Contains(":") ?
                         matchDoc.DocumentNode.SelectSingleNode("//span[3]").InnerText : matchDoc.DocumentNode.SelectSingleNode("//span[4]").InnerText)),

                        AwayScore = matchDoc.DocumentNode.SelectSingleNode("//div[@class='f']//b")?.InnerText.Trim(),

                        HomeLogo = matchDoc.DocumentNode.SelectSingleNode("//div[@class='s']//img")?.GetAttributeValue("data-src", null) is string h ? "http:" + h : null,

                        AwayLogo = matchDoc.DocumentNode.SelectSingleNode("//div[@class='f']//img")?.GetAttributeValue("data-src", null) is string a ? "http:" + a: null,
                        Partition = new Partition()
                        {
                            PartitionName = matchDoc.DocumentNode.SelectSingleNode("//div[@class='cin_cntnr']//a")?.InnerText.Trim()

                        }

                    });
                }
                TeamMatches.Matches = teamMatches;

                return TeamMatches;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetMatchDetailsAsync: {ex.Message}");
                return null;
            }

        }
    }
}
