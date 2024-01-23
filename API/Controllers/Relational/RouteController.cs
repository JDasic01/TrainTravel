// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Extensions.Logging;
// using API.Models;
// using System;
// using System.Linq;
// using System.Threading.Tasks;

// namespace API.Controllers.Relational
// {
//     [ApiController]
//     [Route("[controller]")]
//     public class RouteController : ControllerBase
//     {
//         private readonly ILogger<RouteController> _logger;
//         private readonly PostgreDbContext _dbContext;

//         public RouteController(ILogger<RouteController> logger, PostgreDbContext dbContext)
//         {
//             _logger = logger;
//             _dbContext = dbContext;
//         }

//         [HttpGet]
//         public IActionResult GetRoutes()
//         {
//             var routes = _dbContext.routes.ToList();
//             return Ok(routes);
//         }

//         [HttpGet("{route_id}")]
//         public IActionResult GetRouteById(int route_id)
//         {
//             try
//             {
//                 var route = _dbContext.routes.Find(route_id);
//                 if (route == null)
//                 {
//                     return NotFound("Route not found.");
//                 }

//                 return Ok(route);
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error retrieving route by ID.");
//                 return StatusCode(500, "Internal server error");
//             }
//         }

//         [HttpPost]
//         public async Task<IActionResult> CreateRoute(API.Models.Route route)
//         {
//             try
//             {
//                 _dbContext.routes.Add(route);
//                 await _dbContext.SaveChangesAsync();
//                 return Ok("Route created successfully.");
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error creating route.");
//                 return StatusCode(500, "Internal server error");
//             }
//         }

//         [HttpPut("{route_id}")]
//         public async Task<IActionResult> UpdateRoute(int route_id, API.Models.Route updatedRoute)
//         {
//             try
//             {
//                 var existingRoute = _dbContext.routes.Find(route_id);
//                 if (existingRoute == null)
//                 {
//                     return NotFound("Route not found.");
//                 }

//                 existingRoute.mileage = updatedRoute.mileage;
//                 existingRoute.start_city_id = updatedRoute.start_city_id;
//                 existingRoute.end_city_id = updatedRoute.end_city_id;

//                 await _dbContext.SaveChangesAsync();
//                 return Ok("Route updated successfully.");
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error updating route.");
//                 return StatusCode(500, "Internal server error");
//             }
//         }

//         [HttpDelete("{route_id}")]
//         public async Task<IActionResult> DeleteRoute(int route_id)
//         {
//             try
//             {
//                 var route = _dbContext.routes.Find(route_id);
//                 if (route == null)
//                 {
//                     return NotFound("Route not found.");
//                 }

//                 _dbContext.routes.Remove(route);
//                 await _dbContext.SaveChangesAsync();
//                 return Ok("Route deleted successfully.");
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error deleting route.");
//                 return StatusCode(500, "Internal server error");
//             }
//         }
//     }
// }
