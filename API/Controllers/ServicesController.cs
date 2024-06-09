using API.Models;
using Microsoft.AspNetCore.Mvc;
using Neo4jClient;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ServicesController : ControllerBase
    {
        private readonly ILogger<ServicesController> _logger;
        private readonly IGraphClient _client;
        private readonly TouristGuideService _touristGuideService;
        private readonly WebScrapingService _webScrapingService;

        public ServicesController(
            IGraphClient client,
            ILogger<ServicesController> logger,
            TouristGuideService touristGuideService,
            WebScrapingService webScrapingService
        )
        {
            _logger = logger;
            _client = client;
            _touristGuideService = touristGuideService;
            _webScrapingService = webScrapingService;
        }

        [HttpGet("StartServices")]
        public async void GetTouristGuideInfo()
        {
            _webScrapingService.GetCitiesData();
            _touristGuideService.GetTouristGuideAsync();
        }
    }
}
