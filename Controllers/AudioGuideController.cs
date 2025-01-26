using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AudioGuideController : ControllerBase
    {
        [HttpGet("{fileName}")]
        public IActionResult GetAudioGuide(string fileName)
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, "AudioGuides", fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var bytes = System.IO.File.ReadAllBytes(filePath);
            return File(bytes, "audio/mpeg", fileName);
        }
    }
}
