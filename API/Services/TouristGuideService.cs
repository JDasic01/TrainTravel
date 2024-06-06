using Neo4jClient;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using API.Models;

public class TouristGuideService
{
    private readonly IGraphClient _graphClient;
    private readonly HttpClient _httpClient;
    private readonly string _huggingFaceApiUrl = "https://api-inference.huggingface.co/models/gpt2";
    private readonly string _apiToken;

    public TouristGuideService(IGraphClient graphClient, HttpClient httpClient, string apiToken)
    {
        _graphClient = graphClient;
        _httpClient = httpClient;
        _apiToken = apiToken;
    }

    public async Task<string> GetTouristGuideAsync(string cityName)
    {
        try
        {
            var city = await _graphClient.Cypher
                .Match("(n:City)")
                .Where((City n) => n.city_name == cityName)
                .Return(n => n.As<City>())
                .ResultsAsync;

            if (city == null || !city.Any())
            {
                return $"City {cityName} not found in the database.";
            }

            var cityData = city.FirstOrDefault();
            if (cityData == null)
            {
                return $"City {cityName} not found in the database.";
            }
            else if (cityData.see_text == null || cityData.do_text == null)
            {
                return $"Insufficient data for {cityData.city_name} to generate a tourist guide.";
            }

            var prompt = $"You are a tourist guide for the city {cityData.city_name}. Here are some highlights to see: {cityData.see_text}, and things to do: {cityData.do_text}. Generate a tourist guide plan based on this information.";

            var payload = new
            {
                inputs = prompt,
                parameters = new
                {
                    max_length = 1000 
                }
            };

            var requestContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "");

            var response = await _httpClient.PostAsync(_huggingFaceApiUrl, requestContent);

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<HuggingFaceResponse>(responseContent);

            return result.choices.FirstOrDefault()?.text ?? "No response from the AI model.";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception occurred: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            return "An error occurred while generating the tourist guide.";
        }
    }
}

public class HuggingFaceResponse
{
    public List<HuggingFaceChoice> choices { get; set; }
}

public class HuggingFaceChoice
{
    public string text { get; set; }
}


public class Choice
{
    public string text { get; set; }
}
