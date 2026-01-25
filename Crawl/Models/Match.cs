namespace Crawl.Models
{
    public class Match
    {
        public string HomeTeamName { get; set; }
        public string AwayTeamName { get; set; }
        public string Date { get; set; } 
        public string HomeLogo { get; set; } // "/Date(...)/"
        public string AwayLogo { get; set; } // "/Date(...)/"
        public string HomeScore { get; set; } // "/Date(...)/"
        public string AwayScore { get; set; } // "/Date(...)/"
        public int? HomeTeamScore { get; set; } // "/Date(...)/"
        public int? AwayTeamScore { get; set; } // "/Date(...)/"
        public Partition Partition { get; set; }

    }
}
