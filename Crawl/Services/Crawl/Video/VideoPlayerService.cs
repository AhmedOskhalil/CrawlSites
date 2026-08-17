namespace Crawl.Services.Crawl.Video
{
    public class VideoPlayerService
    {
        public string? VideoUrl { get; private set; }

        public event Action? VideoChanged;

        public void SetVideo(string url)
        {
            VideoUrl = url;
            VideoChanged?.Invoke();
        }
    }
}
