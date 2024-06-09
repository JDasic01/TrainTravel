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
    private readonly string _huggingFaceApiUrl = "https://api-inference.huggingface.co/models/Qiliang/bart-large-cnn-samsum-ChatGPT_v3";
    private readonly string _apiToken;

    public TouristGuideService(IGraphClient graphClient, HttpClient httpClient, string apiToken)
    {
        _graphClient = graphClient;
        _httpClient = httpClient;
        _apiToken = apiToken;
    }

    public async void GetTouristGuideAsync()
    {
        try
        {
            var cities = await _graphClient.Cypher
            .Match("(n:City)")
            .Where("n.english_guide IS NULL")
            .Return(n => n.As<City>())
            .ResultsAsync;

            foreach(var city in cities)
            {

                if (city.see_text == null || city.do_text == null)
                {
                    continue;
                }

                var prompt = $"You are a tourist guide for the city {city.city_name}. Here are some highlights to see: {city.see_text}, and things to do: {city.do_text}. Generate a tourist guide plan based on this information.";

                var payload = new
                {
                    inputs = prompt,
                    parameters = new
                    {
                        max_length = 1000 
                    }
                };

                var requestContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "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");

                var response = await _httpClient.PostAsync(_huggingFaceApiUrl, requestContent);

                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<HuggingFaceResponse>(responseContent);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception occurred: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
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

}