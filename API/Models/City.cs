public class City
{
    public int CityId { get; set; }
    public string CityName { get; set; }

    public ICollection<CityRoute> CityRoutes { get; set; }
}