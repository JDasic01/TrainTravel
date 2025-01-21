using API.Models;
using Neo4jClient;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Amazon;
using Amazon.Polly;
using Amazon.Polly.Model;
using Amazon.Runtime;

public class TextToSpeechService
{
    private readonly IGraphClient _graphClient;
    private readonly HttpClient _httpClient;
    private static string _key;
    private static string _secret;
    private static readonly RegionEndpoint _region = RegionEndpoint.USEast1;

    public TextToSpeechService(IGraphClient graphClient, HttpClient httpClient)
    {
        _graphClient = graphClient;
        _httpClient = httpClient;
    }

    public async Task AudioGuideAsync()
    {
        try
        {
            var cities = await _graphClient.Cypher
                .Match("(n:City)")
                .Where("n.guide_en IS NOT NULL")
                .Return(n => n.As<City>())
                .ResultsAsync;

            foreach (var city in cities)
            {
                Console.WriteLine($"Generating audio guide for {city.city_name}");
                await TextToSpeechLangAsync(city.city_id.ToString(), city.guide_en, "en-US", VoiceId.Joanna);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception occurred: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        }
    }

    private async Task TextToSpeechLangAsync(string cityId, string guideText, string langCode, VoiceId voice)
    {
        var credentials = new BasicAWSCredentials(_key, _secret);

        using (var client = new AmazonPollyClient(credentials, _region))
        {
            var request = new SynthesizeSpeechRequest
            {
                Text = guideText,
                OutputFormat = OutputFormat.Mp3,
                VoiceId = voice,
                LanguageCode = langCode
            };

            try
            {
                var response = await client.SynthesizeSpeechAsync(request);

                string outputFolder = Path.Combine(AppContext.BaseDirectory, "AudioGuides");
                Directory.CreateDirectory(outputFolder);
                string outputFileName = Path.Combine(outputFolder, $"{cityId}-{langCode}.mp3");
                
                using (var output = File.Create(outputFileName))
                {
                    response.AudioStream.CopyTo(output);
                }
                Console.WriteLine($"Audio file created: {outputFileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error synthesizing speech for city ID {cityId}: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
        }
    }
}
