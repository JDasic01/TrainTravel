using System.Text;
using System.Text.Json;
using Neo4jClient;
using API.Models;
using System.Net.Http;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;


namespace API.Services
{
    public class TouristGuideService
    {
        private readonly IGraphClient _graphClient;
        private readonly HttpClient _httpClient;
        private readonly string _huggingFaceApiUrl = "https://api-inference.huggingface.co/models/philschmid/bart-large-cnn-samsum";
        private readonly string _apiToken = "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";

        public TouristGuideService(IGraphClient graphClient, HttpClient httpClient)
        {
            _graphClient = graphClient;
            _httpClient = httpClient;
        }

        public async Task GetTouristGuideAsync()
        {
            var cities = await _graphClient.Cypher
                        .Match("(n:City)")
                        .Where("n.guide_en IS NULL")
                        .Return(n => n.As<City>())
                        .ResultsAsync;

                        foreach(var city in cities)
                        {
                            Console.WriteLine("TouristGuide", city.city_name);

                            if((city.see_text != null || city.do_text != null) && city.guide_en == null)
                            {
                                var guide = await GetCityTouristGuideAsync(city.city_name, city.see_text, city.do_text);
                                city.guide_en = guide;
                                await _graphClient
                                    .Cypher.Match("(c:City)")
                                    .Where((City c) => c.city_id == city.city_id)
                                    .Set("c = $city")
                                    .WithParam("city", city)
                                    .ExecuteWithoutResultsAsync();  
                            }
                        }
        }

        public async Task<string> GetCityTouristGuideAsync(string city_name, string see_text, string do_text)
        {
            try
            {
                var prompt = $"You are a tourist guide for the city {city_name}. Here are some highlights to see: {see_text}, and things to do: {do_text}. Generate a tourist guide plan based on this information.";

                var payload = new
                {
                    inputs = prompt,
                    parameters = new
                    {
                        max_length = 1000 
                    }
                };

                var requestContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiToken);

                var response = await _httpClient.PostAsync(_huggingFaceApiUrl, requestContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<List<HuggingFaceResponse>>(responseContent);
                    var generatedText = result.FirstOrDefault()?.summary_text;

                    return generatedText;
                }
                else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    await Task.Delay(80000); // a bit more than the time it loads the model
                    await GetCityTouristGuideAsync(city_name, see_text, do_text);
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occurred: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
            return null;
        }

        public class HuggingFaceResponse
        {
            public string summary_text { get; set; }
        }
    }
}
