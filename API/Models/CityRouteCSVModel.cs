namespace API.Models;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class CityRouteCSVModel
{
    [Key]
    public int city_id { get; set; }
    [Key]
    public int route_id { get; set; }
}
