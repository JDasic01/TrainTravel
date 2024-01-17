namespace Blazor.Data;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class CityRoute
{
    [Key]
    public int city_id { get; set; }
    [Key]
    public int route_id { get; set; }
}
