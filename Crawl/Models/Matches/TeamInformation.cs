namespace Crawl.Models.Matches
{
    public class TeamInformation
    {
        public int teamId { get; set; } = 0;
        public string TeamName { get; set; }
        public string TeamLogo { get; set; }

        public TeamOrder TeamOrder { get; set; }
        public List<RecentMatch> RecentMatches { get; set; }
    }
}
