using API.Models;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using Neo4jClient;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ServicesTestController : ControllerBase
    {
        private readonly IGraphClient _client;
        private readonly WebScrapingService _webScrapingService;
        private readonly TouristGuideService _touristGuideService;

        public ServicesTestController(
            IGraphClient client,
            WebScrapingService webScrapingService,
            TouristGuideService touristGuideService
        )
        {
            _client = client;
            _webScrapingService = webScrapingService;
            _touristGuideService = touristGuideService;
        }

        [HttpPost("WebscrapingCities")]
        public async Task<IActionResult> WebscrapingCities()
        {
            _webScrapingService.GetCitiesData();
            return Ok("Web scraping for cities initiated.");
        }

        [HttpPost("WebscrapingStations")]
        public async Task<IActionResult> WebscrapingStations()
        {
            _webScrapingService.ScrapeStations();
            return Ok("Web scraping for stations initiated.");
        }

        [HttpPost("TouristGuideGeneration")]
        public async Task<IActionResult> TouristGuideGeneration()
        {
            _touristGuideService.GetTouristGuideAsync();
            return Ok("Tourist guide generation initiated.");
        }
    }
}
