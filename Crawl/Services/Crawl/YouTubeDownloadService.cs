using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;
using System.Net;
using System.Net.Http.Headers;

namespace Crawl.Services.Crawl
{
    public class YouTubeDownloadService
    {
        private readonly YoutubeClient _youtube;

        public YouTubeDownloadService()
        {
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                UseCookies = true
            };

            var httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromMinutes(5)
            };

            // Realistic browser headers
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
                "AppleWebKit/537.36 (KHTML, like Gecko) " +
                "Chrome/122.0.0.0 Safari/537.36");

            httpClient.DefaultRequestHeaders.AcceptLanguage
                .Add(new StringWithQualityHeaderValue("en-US"));
            httpClient.DefaultRequestHeaders.AcceptLanguage
                .Add(new StringWithQualityHeaderValue("en", 0.9));

            _youtube = new YoutubeClient(httpClient);
        }

        // ---------------------------
        // GET AVAILABLE QUALITIES
        // ---------------------------
        public async Task<List<string>> GetAvailableQualitiesAsync(
            string url,
            CancellationToken ct = default)
        {
            var video = await _youtube.Videos.GetAsync(url, ct);

            var manifest = await ExecuteWithRetry<StreamManifest>(
                () => _youtube.Videos.Streams.GetManifestAsync(video.Id, ct));

            return manifest
                .GetMuxedStreams()
                .Select(s => s.VideoQuality.Label)
                .Distinct()
                .OrderByDescending(q => q)
                .ToList();
        }

        // ---------------------------
        // DOWNLOAD VIDEO
        // ---------------------------
        public async Task<string> DownloadAsync(
            string url,
            string quality,
            string savePath,
            IProgress<double>? progress = null,
            CancellationToken ct = default)
        {
            var video = await _youtube.Videos.GetAsync(url, ct);

            var manifest = await ExecuteWithRetry<StreamManifest>(
                () => _youtube.Videos.Streams.GetManifestAsync(video.Id, ct));

            var streamInfo = manifest
                .GetMuxedStreams()
                .Where(s => s.VideoQuality.Label == quality)
                .OrderByDescending(s => s.VideoQuality.MaxHeight)
                .FirstOrDefault();

            if (streamInfo == null)
                throw new Exception("Requested quality not found.");

            Directory.CreateDirectory(savePath);

            var filePath = Path.Combine(savePath, $"{SanitizeFileName(video.Title)}.mp4");

            await _youtube.Videos.Streams.DownloadAsync(streamInfo, filePath, progress, ct);

            return filePath;
        }

        // ---------------------------
        // RETRY LOGIC (ValueTask-compatible)
        // ---------------------------
        private async Task<T> ExecuteWithRetry<T>(Func<ValueTask<T>> action, int retries = 3)
        {
            Exception? lastEx = null;

            for (int i = 0; i < retries; i++)
            {
                try
                {
                    return await action();
                }
                catch (Exception ex)
                {
                    lastEx = ex;
                    await Task.Delay(1500);
                }
            }

            throw lastEx ?? new Exception("Unknown retry error.");
        }

        // ---------------------------
        // SAFE FILENAME
        // ---------------------------
        private string SanitizeFileName(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');

            return name;
        }
    }
}
