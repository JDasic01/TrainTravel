using API.Models;
using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using API.Services;

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
        private readonly TranslationService _translationService;
        private readonly TextToSpeechService _textToSpeechService;

        public ServicesController(
            IGraphClient client,
            ILogger<ServicesController> logger,
            TouristGuideService touristGuideService,
            WebScrapingService webScrapingService,
            TranslationService translationService,
            TextToSpeechService textToSpeechService
        )
        {
            _logger = logger;
            _client = client;
            _touristGuideService = touristGuideService;
            _webScrapingService = webScrapingService;
            _textToSpeechService = textToSpeechService;
        }

        [HttpGet("StartServices")]
        public async void GetTouristGuideInfo()
        {
            // _webScrapingService.GetCitiesData();
            // await _touristGuideService.GetTouristGuideAsync();
            // await _textToSpeechService.AudioGuideAsync();
        }
    }
}
