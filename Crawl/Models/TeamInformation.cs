namespace Crawl.Models
{
    public class TeamInformation
    {
        public string TeamName { get; set; }
        public string TeamLogo { get; set; }

        public TeamOrder TeamOrder { get; set; }
        public List<RecentMatch> RecentMatches { get; set; }
    }
}
