namespace Crawl.Models.Matches
{
    public class Partition
    {
        public string? PartitionName { get; set; }
        public List<Match> Matchs { get; set; } = new();
    }
}
