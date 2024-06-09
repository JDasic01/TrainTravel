using API.Models;
using HtmlAgilityPack;
using Neo4jClient;
using System.Text;
using Newtonsoft.Json;

public class WebScrapingService
{
    private readonly IGraphClient _graphClient;
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl = "https://www.wikitravel.org/en/";

    public WebScrapingService(IGraphClient graphClient, HttpClient httpClient)
    {
        _graphClient = graphClient;
        _httpClient = httpClient;
    }

    public async void GetCitiesData()
    {
        try
        {
            var cities = await _graphClient.Cypher
                .Match("(n:City)")
                .Where("n.doext IS NULL OR n.see_text IS NULL")
                .Return(n => n.As<City>())
                .ResultsAsync;

            foreach (var city in cities)
            {
                Console.WriteLine($"City: {city.name}");
                var url = _baseUrl + city.name;
                var response = await _httpClient.GetAsync(url);
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    continue;
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(responseBody);

                var seeSectionContent = GetSectionContent(htmlDoc, "//*[@id='See']");
                var doSectionContent = GetSectionContent(htmlDoc, "//*[@id='Do']");

                (seeSectionContent, doSectionContent) = EnsureTokenLimit(seeSectionContent, doSectionContent);
                Console.WriteLine($"see: {seeSectionContent}, do {doSectionContent}");
                await SaveCitySectionsToDb(city.name, seeSectionContent, doSectionContent);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception occurred: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        }
    }

    private static string GetSectionContent(HtmlDocument htmlDoc, string sectionXPath)
    {
        var sectionNode = htmlDoc.DocumentNode.SelectSingleNode(sectionXPath);
        if (sectionNode != null)
        {
            var contentBuilder = new StringBuilder();
            var siblingNode = sectionNode.ParentNode.NextSibling;
            while (siblingNode != null && siblingNode.Name != "h2")
            {
                contentBuilder.Append(siblingNode.InnerText.Trim());
                siblingNode = siblingNode.NextSibling;
            }
            return contentBuilder.ToString().Trim();
        }
        return null;
    }

    private (string, string) EnsureTokenLimit(string seeContent, string doContent)
    {
        int tokenLimit = 850;

        int seeContentTokens = seeContent.Split(' ').Length;
        int doContentTokens = doContent.Split(' ').Length;

        int totalTokens = seeContentTokens + doContentTokens;

        if (totalTokens > tokenLimit)
        {
            int excessTokens = totalTokens - tokenLimit;
            int seeContentLimit = Math.Max(seeContentTokens - (excessTokens / 2), 0);
            int doContentLimit = Math.Max(doContentTokens - (excessTokens / 2), 0);
            seeContent = string.Join(' ', seeContent.Split(' ').Take(seeContentLimit));
            doContent = string.Join(' ', doContent.Split(' ').Take(doContentLimit));

            totalTokens = seeContent.Split(' ').Length + doContent.Split(' ').Length;

            if (totalTokens > tokenLimit)
            {
                int finalExcessTokens = totalTokens - tokenLimit;
                if (seeContent.Split(' ').Length > doContent.Split(' ').Length)
                {
                    seeContent = string.Join(' ', seeContent.Split(' ').Take(seeContent.Split(' ').Length - finalExcessTokens));
                }
                else
                {
                    doContent = string.Join(' ', doContent.Split(' ').Take(doContent.Split(' ').Length - finalExcessTokens));
                }
            }
        }

        return (seeContent, doContent);
    }

    private async Task SaveCitySectionsToDb(string cityName, string seeContent, string doContent)
    {
        await _graphClient.Cypher
            .Match("(c:City {name: $cityName})")
            .Set("c.see_text = $seeContent, c.do_text = $doContent")
            .WithParams(new { cityName, seeContent, doContent })
            .ExecuteWithoutResultsAsync();
    }

    public async void ScrapeStations()
    {
        try
        {
            var query = @"
                [out:json];
                (
                    node['railway'='station'](42.3932,13.4937,46.5557,19.4269);
                );
                out body;
            ";

            var httpClient = new HttpClient();
            var content = new StringContent(query);

            var response = await httpClient.PostAsync("https://overpass-api.de/api/interpreter", content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            dynamic stations = JsonConvert.DeserializeObject(responseBody);
            Console.WriteLine(responseBody);

            foreach (var station in stations.elements)
            {
                string name = station.tags.name;
                double lat = station.lat;
                double lon = station.lon;

                var queryGeocoding = @"
                    [out:json];
                    (
                        node(around:1000,lat,lon)['place'~'city|town'];
                        node(around:1000,lat,lon)['place'='country'];
                    );
                    out center;
                ".Replace("lat", lat.ToString()).Replace("lon", lon.ToString());

                var contentGeocoding = new StringContent(queryGeocoding);
                var resp = await httpClient.PostAsync("https://overpass-api.de/api/interpreter", contentGeocoding);
                resp.EnsureSuccessStatusCode();

                var respBody = await resp.Content.ReadAsStringAsync();
                dynamic result = JsonConvert.DeserializeObject(respBody);
                Console.WriteLine(respBody);

                var address = result.elements?[0].tags;
                var country = address?.country?.ToString();
                var town = address?.town?.ToString();

                var stat = new Station(){
                    name = name,
                    latitude = lat,
                    longitude = lon,
                    country = country,
                    town = town
                };

                await _graphClient.Cypher
                    .Create("(s:Station $station)")
                    .WithParam("station", stat)
                    .ExecuteWithoutResultsAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}
