namespace API.Models;

public class LineCSVModel
{
    public int id { get; set; }
    public string name { get; set; }
    public int startCityId { get; set; }
    public int endCityId { get; set; }
}
