namespace API.Models;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class TrainRoute
{
    public int line_id { get; set; }
    public List<int> city_ids { get; set; }
    public List<int> mileage { get; set; }
}
