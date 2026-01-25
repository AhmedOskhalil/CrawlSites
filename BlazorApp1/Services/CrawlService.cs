using System.Text.Json;

namespace CrawlFilgoal.Services
{
    public class CrawlService
    {
        private  readonly HttpClient _httpClient = new HttpClient();

        public  async Task RunAsync()
        {
            var url = "https://www.filgoal.com/matches/?date=2026-01-06";

            var request = new HttpRequestMessage(HttpMethod.Get, url);

            // Headers
            request.Headers.Add("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8");
            request.Headers.Add("accept-language", "en-US,en;q=0.9");
            request.Headers.Add("cache-control", "max-age=0");
            request.Headers.Add("referer", "https://www.google.com/");
            request.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/143.0.0.0 Safari/537.36");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var html = await response.Content.ReadAsStringAsync();

            // Extract JS object
            var startToken = "viewModelData = ";
            var endToken = "}]}];";

            var startIndex = html.IndexOf(startToken);
            if (startIndex == -1)
                throw new Exception("viewModelData not found");

            startIndex += startToken.Length;

            var endIndex = html.IndexOf(endToken, startIndex);
            if (endIndex == -1)
                throw new Exception("End of viewModelData not found");

            var jsonText = html.Substring(startIndex, endIndex - startIndex) + endToken;

            // Parse JSON
            using var jsonDoc = JsonDocument.Parse(jsonText);

            // Pretty print
            var formattedJson = JsonSerializer.Serialize(
                jsonDoc.RootElement,
                new JsonSerializerOptions { WriteIndented = true }
            );

            Console.WriteLine(formattedJson);

        }

    }
}
