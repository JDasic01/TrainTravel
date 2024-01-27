namespace API.Models;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class LineCSVModel
{
    public int line_id { get; set; }
    public string line_name { get; set; }
    public int start_city_id { get; set; }
    public int end_city_id { get; set; }
}
