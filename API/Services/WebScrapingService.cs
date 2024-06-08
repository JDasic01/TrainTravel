using API.Models;
using HtmlAgilityPack;
using Neo4jClient;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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
                .Where("n.do_text IS NULL OR n.see_text IS NULL")
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

                (seeSectionContent, doSectionContent) = EnsureTokenLimit(city.name, seeSectionContent, doSectionContent);
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

    private (string, string) EnsureTokenLimit(string cityName, string seeContent, string doContent)
    {
        int tokenLimit = 850;

        int cityNameTokens = cityName.Split(' ').Length;
        int seeContentTokens = seeContent.Split(' ').Length;
        int doContentTokens = doContent.Split(' ').Length;

        int totalTokens = cityNameTokens + seeContentTokens + doContentTokens;

        if (totalTokens > tokenLimit)
        {
            int excessTokens = totalTokens - tokenLimit;
            int seeContentLimit = Math.Max(seeContentTokens - (excessTokens / 2), 0);
            int doContentLimit = Math.Max(doContentTokens - (excessTokens / 2), 0);
            seeContent = string.Join(' ', seeContent.Split(' ').Take(seeContentLimit));
            doContent = string.Join(' ', doContent.Split(' ').Take(doContentLimit));

            totalTokens = cityNameTokens + seeContent.Split(' ').Length + doContent.Split(' ').Length;

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
        var url = "https://www.hzpp.hr/";
        var response = await _httpClient.GetAsync(url);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            Console.WriteLine("URL not found.");
            return;
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        HtmlDocument htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(responseBody);

        // Get all data from the list //*[@id='ui-id-1']
        var stationNodes = htmlDoc.DocumentNode.SelectNodes("//*[@id='ui-id-1']//li/a");
        
        if (stationNodes != null)
        {
            foreach (var node in stationNodes)
            {
                var stationName = node.InnerText.Trim();
                Console.WriteLine(stationName);
                // Save the station to the database
                await SaveStationToDb(stationName);
            }
        }
        else
        {
            Console.WriteLine("No stations found.");
        }
    }

    private async Task SaveStationToDb(string stationName)
    {
        await _graphClient.Cypher
            .Merge("(s:Station {station_name: $stationName})")
            .OnCreate()
            .Set("s.created_at = timestamp()")
            .WithParam("stationName", stationName)
            .ExecuteWithoutResultsAsync();
    }

    public class HuggingFaceResponse
    {
        public List<HuggingFaceChoice> choices { get; set; }
    }

    public class HuggingFaceChoice
    {
        public string text { get; set; }
    }
}
