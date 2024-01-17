using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CsvHelper;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using API.Models;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CityController : ControllerBase
    {
        private readonly ILogger<CityController> _logger;
        private readonly PostgreDbContext _dbContext; 

        public CityController(ILogger<CityController> logger, PostgreDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult GetCities()
        {
            var cities = _dbContext.cities.ToList();
            return Ok(cities);
        }
        
        [HttpGet("{city_id}")]
        public IActionResult GetCityById(int city_id)
        {
            try
            {
                var city = _dbContext.cities.Find(city_id);
                if (city == null)
                {
                    return NotFound("City not found.");
                }

                return Ok(city);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving city by ID.");
                return StatusCode(500, "Internal server error");
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> CreateCity(City city)
        {
            try
            {
                _dbContext.cities.Add(city);
                await _dbContext.SaveChangesAsync();
                return Ok("City created successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating city.");
                return StatusCode(500, "Internal server error");
            }
        }
        
        [HttpPut("{city_id}")]
        public async Task<IActionResult> UpdateCity(int city_id, City updatedCity)
        {
            try
            {
                var existingCity = _dbContext.cities.Find(city_id);
                if (existingCity == null)
                {
                    return NotFound("City not found.");
                }

                existingCity.city_name = updatedCity.city_name;

                await _dbContext.SaveChangesAsync();
                return Ok("City updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating city.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{city_id}")]
        public async Task<IActionResult> DeleteCity(int city_id)
        {
            try
            {
                var city = _dbContext.cities.Find(city_id);
                if (city == null)
                {
                    return NotFound("City not found.");
                }

                _dbContext.cities.Remove(city);
                await _dbContext.SaveChangesAsync();
                return Ok("City deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting city.");
                return StatusCode(500, "Internal server error");
            }
        }
    }   
}