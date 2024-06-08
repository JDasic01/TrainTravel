namespace API.Models;

public class Line
{
    public int id { get; set; }
    public string name { get; set; }
    public int startCityId { get; set; }
    public int endCityId { get; set; }
}