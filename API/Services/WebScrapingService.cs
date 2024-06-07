using API.Models;
using Neo4jClient;
using HtmlAgilityPack;
using System.Text;

public class WebScrapingService
{
    private readonly IGraphClient _graphClient;
    private readonly HttpClient _httpClient;
    private readonly string _base_url = "https://www.wikitravel.org/en/";
    private readonly string _apiToken;

    public WebScrapingService(IGraphClient graphClient, HttpClient httpClient, string apiToken)
    {
        _graphClient = graphClient;
        _httpClient = httpClient;
        _apiToken = apiToken;
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

            foreach(var city in cities)
            {
                var url = _base_url + city.city_name;
                HttpClient client = new HttpClient();
                var response = await client.GetAsync(url);
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return;
                }
            
                var responseBody = await response.Content.ReadAsStringAsync();
                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(responseBody);

                var seeSectionContent = GetSectionContent(htmlDoc, "//*[@id=\"See\"]");
                var doSectionContent = GetSectionContent(htmlDoc, "//*[@id=\"Do\"]");

                (seeSectionContent, doSectionContent) = EnsureTokenLimit(city.city_name, seeSectionContent, doSectionContent);

                await SaveCitySectionsToDb(city.city_name, seeSectionContent, doSectionContent);
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
                .Match("(c:City {city_name: $cityName})")
                .Set("c.see_text = $seeContent, c.do_text = $doContent")
                .WithParams(new { cityName, seeContent, doContent })
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