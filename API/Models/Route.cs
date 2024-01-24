namespace API.Models;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Route
{
    [Key]
    public int route_id { get; set; }
    public decimal mileage { get; set; }
    [ForeignKey("StartCity")]
    public int start_city_id { get; set; }
    [ForeignKey("EndCity")]
    public int end_city_id { get; set; }
    
    public City StartCity { get; set; }
    public City EndCity { get; set; }

    public ICollection<CityRoute> city_routes { get; set; } = new List<CityRoute>();
}
