using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GPT4AllController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public GPT4AllController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] PromptRequest request)
        {
            var payload = GeneratePayload(request.Prompt, request.CityName, request.Text);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("http://gpt4all:5000/generate", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return Ok(responseContent);
            }

            return StatusCode((int)response.StatusCode, response.ReasonPhrase);
        }

        private string GeneratePayload(string prompt, string cityName, string text)
        {
            var modifiedPrompt = $"{prompt}\n\nYou are a virtual tourist guide for the city of {cityName}. Your job is to provide detailed and engaging information about the various attractions, activities, and historical sites in {cityName}.\n\nBase the information on following text:\n{text}";
            return JsonSerializer.Serialize(new { prompt = modifiedPrompt, temp = 0 });
        }
    }

    public class PromptRequest
    {
        public string Prompt { get; set; }
        public string CityName { get; set; }
        public string Text { get; set; }
    }
}
