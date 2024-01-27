namespace Blazor.Data;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class TrainRoute
{
    public int route_id { get; set; }
    public int line_id { get; set; }
    public List<int> cities { get; set; }
    public decimal total_mileage { get; set; }
}
