namespace API.Models;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Line
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int StartCityId { get; set; }
    public int EndCityId { get; set; }
}