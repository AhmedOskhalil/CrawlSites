namespace Crawl.Models
{
    public class TeamMatches
    {
        public string TeamName { get; set; }
        public string TeamLogo { get; set; }
        public List<Match>? Matches { get; set; }
    }
}
