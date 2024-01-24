namespace API.Models;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class City
{
    [Key]
    public int city_id { get; set; }
    public string city_name { get; set; }
    public ICollection<CityRoute> city_routes { get; set; } = new List<CityRoute>();
    public ICollection<CityToCity> city_to_city { get; set; } = new List<CityToCity>();
}