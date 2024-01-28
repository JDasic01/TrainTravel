namespace API.Models;

using System.Collections.Generic;

public class TrainRoute
{
    public int route_id { get; set; }
    public int line_id { get; set; }
    public List<int> city_ids { get; set; }
    public List<int> mileage { get; set; }
}
