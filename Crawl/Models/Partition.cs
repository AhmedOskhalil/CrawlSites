namespace Crawl.Models
{
    public class Partition
    {
        public string? PartitionName { get; set; }
        public List<Match> Matchs { get; set; } = new();
    }
}
