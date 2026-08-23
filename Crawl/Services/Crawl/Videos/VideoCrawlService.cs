using Crawl.Models.Vidoes;
using Crawl.Services.Crawl.Helpers;

namespace Crawl.Services.Crawl.Videos
{
    public class VideoCrawlService
    {
        private static readonly HttpClient _httpClient = new();
        private readonly CrawlHelpersService _helpersService;
        public VideoCrawlService(CrawlHelpersService helpersService)
        {
            _helpersService = helpersService;
            if (!_httpClient.DefaultRequestHeaders.Contains("referer"))
            {
                _httpClient.DefaultRequestHeaders.Add("referer", "https://www.google.com/");
            }

            if (!_httpClient.DefaultRequestHeaders.Contains("user-agent"))
            {
                _httpClient.DefaultRequestHeaders.Add(
                    "user-agent",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0 Safari/537.36"
                );
            }

            _helpersService = helpersService;
        }

        #region vidoes
        public async Task<List<VideoItem>> GetVideosAsync()
        {
            try
            {
                var html = await _helpersService.GetHtmlAsync("https://www.filgoal.com/videos");

                var doc = _helpersService.LoadDocument(html);

                var videoNodes = doc.DocumentNode
                    .SelectNodes("//div[@class='vfg_item']");

                var videos = new List<VideoItem>();

                if (videoNodes == null)
                    return videos;

                foreach (var node in videoNodes)
                {
                    var videoDoc = _helpersService.LoadDocument(node.InnerHtml);

                    var links = videoDoc.DocumentNode.SelectNodes("//a");

                    var video = new VideoItem
                    {
                        Title = videoDoc.DocumentNode
                            .SelectSingleNode("//span[@itemprop='name']")
                            ?.InnerText
                            ?.Trim() ?? "",

                        Url = links?[0]
                            ?.GetAttributeValue("href", "") ?? "",

                        Thumbnail = _helpersService.FixImageUrl(
                            videoDoc.DocumentNode
                            .SelectSingleNode("//img")
                            ?.GetAttributeValue("data-src", "")
                        ) ?? "",

                        PublishDate = links?[1]
                            ?.SelectNodes(".//span")?[1]
                            ?.InnerText
                            ?.Trim() ?? ""
                    };

                    videos.Add(video);
                }

                return videos;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetVideosAsync Error: {ex.Message}");

                return new List<VideoItem>();
            }
        }

        public async Task<string?> GetVideoUrlAsync(string videoUrl)
        {
            try
            {
                var html = await _helpersService.GetHtmlAsync(videoUrl);

                var doc = _helpersService.LoadDocument(html);

                var iframe = doc.DocumentNode.SelectSingleNode("//div[@class='v_object reel']//iframe");
                var vidframelink = doc.DocumentNode.SelectSingleNode("//div[@id=\"details_content\"]").InnerHtml.ToString().Split("<iframe src=")[1].Split(" ")[0];
                if (iframe == null)
                {
                    if (!string.IsNullOrEmpty(vidframelink))
                        return vidframelink.Replace("\"", "");
                    return null;
                }

                return iframe.GetAttributeValue("src", null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetVideoUrlAsync Error: {ex.Message}");
                return null;
            }
        }
        #endregion
    }
}
