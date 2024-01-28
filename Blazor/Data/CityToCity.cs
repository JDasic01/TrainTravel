namespace Blazor.Data;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class CityToCity
{
    [Key]
    public int city_id_1 { get; set; }
    [Key]
    public int city_id_2 { get; set; }

    public decimal mileage { get; set; }
}
