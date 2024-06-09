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

        public ServicesController(
            IGraphClient client,
            ILogger<ServicesController> logger
        )
        {
            _logger = logger;
            _client = client;
        }
    }
}
