namespace API.Models;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class City
{
    public int city_id { get; set; }
    public string city_name { get; set; }
    public string see_text { get; set; }
    public string do_text { get; set; }
}