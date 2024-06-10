using API.Models;
using Neo4jClient;
using System.Text;
using Newtonsoft.Json.Linq;
using Amazon;
using Amazon.Auth;
using Amazon.Runtime;
using Amazon.Translate;
using Amazon.Translate.Model;

namespace API.Services;

public class TranslationService
{
    private readonly IGraphClient _graphClient;
    private readonly HttpClient _httpClient;

    public TranslationService(IGraphClient graphClient, HttpClient httpClient)
    {
        _graphClient = graphClient;
        _httpClient = httpClient;
    }

    public async void TranslateGuide()
    {
        try
        {
            var cities = await _graphClient.Cypher
                .Match("(n:City)")
                .Where("n.guide_en IS NOT NULL")
                .Return(n => n.As<City>())
                .ResultsAsync;

            foreach(var city in cities)
            {
                Console.WriteLine("Translation", city.city_name);
                
                if(city.guide_esp == null)
                {
                    city.guide_esp = TranslateToLang(city.guide_en, "es");
                }
                else if(city.guide_ger == null)
                {
                    city.guide_ger = TranslateToLang(city.guide_en, "de");
                }

                    await _graphClient.Cypher
                        .Match("(c:City)")
                        .Where((City c) => c.city_id == city.city_id)
                        .Set("c = $city")
                        .WithParam("city", city)
                        .ExecuteWithoutResultsAsync();
            }    
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception occurred: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        }
    }

    public static string TranslateToLang(string guideText, string langCode)
    {
        var region = RegionEndpoint.USEast1;
        var credentials = new BasicAWSCredentials(_key, _secret);

        using(var client = new AmazonTranslateClient(credentials, region))
        {   
            var request = new Amazon.Translate.Model.TranslateTextRequest();

            request.Text = guideText;
            request.TargetLanguageCode = langCode;
            request.SourceLanguageCode = "EN";
            var response = client.TranslateTextAsync(request).Result;
            Console.WriteLine(response.TranslatedText);
            return response.TranslatedText;
        }
    }
}
