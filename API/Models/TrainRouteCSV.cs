namespace API.Models;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class TrainRouteCSV
{
    public int line_id { get; set; }
    public string city_ids { get; set; }
    public string mileage { get; set; }
}
