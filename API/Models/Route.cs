public class Route
{
    public int RouteId { get; set; }
    public decimal Mileage { get; set; }

    public int StartCityId { get; set; }
    public City StartCity { get; set; }

    public int EndCityId { get; set; }
    public City EndCity { get; set; }

    public ICollection<CityRoute> CityRoutes { get; set; }
}
