using Crawl.Models;
using System.Text.Json;
using HtmlAgilityPack;



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

    }
}
