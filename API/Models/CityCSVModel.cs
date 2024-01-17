namespace API.Models;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class CityCSVModel
{
    [Key]
    public int city_id { get; set; }
    public string city_name { get; set; }
    public ICollection<CityRoute> city_routes { get; set; }
}
