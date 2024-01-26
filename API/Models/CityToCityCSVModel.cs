namespace API.Models;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class CityToCityCSVModel
{
    [Key]
    public int city_id_1 { get; set; }
    [Key]
    public int city_id_2 { get; set; }

    public decimal mileage { get; set; }
}
