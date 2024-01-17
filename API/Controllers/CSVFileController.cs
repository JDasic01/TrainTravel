using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CsvHelper;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [ApiController]
    [Route("csv-file-upload")]
    public class CSVFileController : ControllerBase
    {
        private readonly ILogger<CSVFileController> _logger;
        private readonly PostgreDbContext _dbContext; 

        public CSVFileController(ILogger<CSVFileController> logger, PostgreDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        [HttpPost("upload-cities", Name = "UploadCitiesCSV")]
        public async Task<IActionResult> CreateCities(IFormFile formFile)
        {
            try
            {
                using (var reader = new StreamReader(formFile.OpenReadStream()))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var cityRecords = csv.GetRecords<CityCSVModel>();
                    foreach (var cityRecord in cityRecords)
                        {
                            var city = new City
                            {
                                city_id = cityRecord.city_id,
                                city_name = cityRecord.city_name,
                                city_routes = new HashSet<CityRoute>()
                            };
                            _dbContext.cities.Add(city);
                        }
                    await _dbContext.SaveChangesAsync();    
                }

                return Ok("CSV file for cities uploaded successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing CSV file.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("upload-routes", Name = "UploadRoutesCSV")]
        public async Task<IActionResult> CreateRoutes(IFormFile formFile)
        {
            try
            {
                using (var reader = new StreamReader(formFile.OpenReadStream()))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var routeRecords = csv.GetRecords<RouteCSVModel>();
                    foreach (var routeRecord in routeRecords)
                    {
                        var route = new API.Models.Route
                        {
                            route_id = routeRecord.route_id,
                            mileage = routeRecord.mileage,
                            start_city_id = routeRecord.start_city_id,
                            end_city_id = routeRecord.end_city_id
                        };
                        
                        _dbContext.routes.Add(route);
                        await _dbContext.SaveChangesAsync();

                        var startCity = _dbContext.cities.Find(routeRecord.start_city_id);
                        var endCity = _dbContext.cities.Find(routeRecord.end_city_id);
                        
                        if (startCity == null || endCity == null)
                        {
                            Console.WriteLine("Start city or end city not found. Skipping route creation.");
                            continue; 
                        }

                        var cityRoute = new CityRoute
                        {
                            city_id = startCity.city_id,
                            route_id = route.route_id
                        };

                        startCity.city_routes.Add(cityRoute);
                        endCity.city_routes.Add(cityRoute);

                        _dbContext.Entry(startCity).State = EntityState.Modified;
                        _dbContext.Entry(endCity).State = EntityState.Modified;

                        _dbContext.cityroutes.Add(cityRoute);
                    }

                    await _dbContext.SaveChangesAsync();
                }

                return Ok("CSV file for routes uploaded successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing CSV file.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("delete-data", Name = "DeleteDataFromDb")]
        public async Task<IActionResult> DeleteAll()
        {
            try
            {
                _dbContext.cityroutes.RemoveRange(_dbContext.cityroutes);
                _dbContext.routes.RemoveRange(_dbContext.routes);
                _dbContext.cities.RemoveRange(_dbContext.cities);
                await _dbContext.SaveChangesAsync();    
                return Ok("All data deleted sucessfuly");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting data.");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
