using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class TouristGuideController : ControllerBase
{
    private readonly TouristGuideService _touristGuideService;

    public TouristGuideController(TouristGuideService touristGuideService)
    {
        _touristGuideService = touristGuideService;
    }

    [HttpGet("{cityName}")]
    public async Task<IActionResult> GetTouristGuide(string cityName)
    {
        var guide = await _touristGuideService.GetTouristGuideAsync(cityName);
        return Ok(guide);
    }
}
