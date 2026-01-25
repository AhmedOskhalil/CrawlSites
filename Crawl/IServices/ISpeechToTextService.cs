namespace Crawl.IServices
{
    public interface ISpeechToTextService
    {
        Task<string> TranscribeAsync(Stream fileStream, string fileName);
    }
}
