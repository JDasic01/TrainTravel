namespace API.Models;

using System.Collections.Generic;

public class TrainRoute
{
    public int id { get; set; }
    public int lineId { get; set; }
    public List<int> citiesIds { get; set; }
    public List<int> mileage { get; set; }
}
