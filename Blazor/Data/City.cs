namespace Blazor.Data;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class City
{
    public int city_id { get; set; }
    public string city_name { get; set; }
}

public class AvailableRoute
{
    public int lineId { get; set; }
    public int endCityId { get; set; }
    public string endCityName { get; set; }
}

public class GetCity
{
    public int cityId { get; set; }
    public string cityName { get; set; }
    public List<AvailableRoute> availableRoutes { get; set; }
}