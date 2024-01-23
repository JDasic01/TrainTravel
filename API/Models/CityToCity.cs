namespace API.Models;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class CityToCity
{
    [Key]
    public int city_id1 { get; set; }
    [Key]
    public int city_id2 { get; set; }

    public decimal mileage { get; set; }
}