using Crawl.IServices;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System.Text;
using Xabe.FFmpeg;

namespace Crawl.Services
{
    public class SpeechToTextService : ISpeechToTextService
    {
        private readonly string _speechKey;
        private readonly string _speechRegion;

        public SpeechToTextService(IConfiguration config)
        {
            _speechKey = config["AzureSpeech:Key"]
                ?? throw new Exception("Azure Speech Key missing");

            _speechRegion = config["AzureSpeech:Region"]
                ?? throw new Exception("Azure Speech Region missing");
        }

        public async Task<string> TranscribeAsync(Stream fileStream, string fileName)
        {
            var tempInput = Path.Combine(Path.GetTempPath(), fileName);
            var audioFile = tempInput;

            // Save uploaded file
            await using (var fs = File.Create(tempInput))
                await fileStream.CopyToAsync(fs);

            // Convert video → WAV (Azure prefers WAV PCM)
            if (IsVideo(fileName))
            {
                audioFile = Path.ChangeExtension(tempInput, ".wav");

                FFmpeg.SetExecutablesPath(
                    @"C:\Users\78sworks\Downloads\ffmpeg-8.0.1-essentials_build\ffmpeg-8.0.1-essentials_build\bin");

                await FFmpeg.Conversions.New()
                    .AddParameter($"-i \"{tempInput}\" -ar 16000 -ac 1 \"{audioFile}\"")
                    .Start();
            }

            var speechConfig = SpeechConfig.FromSubscription(_speechKey, _speechRegion);

            // 🔥 Arabic + English auto detection
            speechConfig.SetProperty(
                PropertyId.SpeechServiceConnection_LanguageIdMode,
                "Continuous");

            var autoLangConfig = AutoDetectSourceLanguageConfig.FromLanguages(
                new[] { "ar-EG", "en-US" });

            using var audioConfig = AudioConfig.FromWavFileInput(audioFile);
            using var recognizer = new SpeechRecognizer(
                speechConfig,
                autoLangConfig,
                audioConfig);

            var resultBuilder = new StringBuilder();

            recognizer.Recognized += (_, e) =>
            {
                if (e.Result.Reason == ResultReason.RecognizedSpeech)
                {
                    resultBuilder.AppendLine(e.Result.Text);
                }
            };

            await recognizer.StartContinuousRecognitionAsync();
            await Task.Delay(TimeSpan.FromSeconds(5));
            await recognizer.StopContinuousRecognitionAsync();

            var finalText = resultBuilder.ToString();

            if (string.IsNullOrWhiteSpace(finalText))
                throw new Exception("No speech detected");

            // Save output file
            var outputPath = Path.Combine(
                "wwwroot",
                "transcriptions",
                $"{Path.GetFileNameWithoutExtension(fileName)}.txt");

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
            await File.WriteAllTextAsync(outputPath, finalText, Encoding.UTF8);

            return outputPath;
        }

        private static bool IsVideo(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLower();
            return ext is ".mp4" or ".avi" or ".mkv";
        }
    }
}
