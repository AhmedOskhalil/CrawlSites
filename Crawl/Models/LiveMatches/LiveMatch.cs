using Crawl.Models.Matches;

namespace Crawl.Models.LiveMatches
{
    public class LiveMatch
    {
            public string HomeTeamName { get; set; }
            public string AwayTeamName { get; set; }
            public string Date { get; set; }
            public string HomeLogo { get; set; } 
            public string AwayLogo { get; set; } 
            public string HomeScore { get; set; } 
            public string AwayScore { get; set; } 
            public int? HomeTeamScore { get; set; } 
            public int? AwayTeamScore { get; set; } 
            public Partition Partition { get; set; }
            public bool IsFinished { get; set; } = false;        
            public bool IsRunning { get; set; } = false;

    }
}
