namespace API.Models;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Route
{
    public int route_id { get; set; }
    public decimal mileage { get; set; }
    public int start_city_id { get; set; }
    public int end_city_id { get; set; }
}
