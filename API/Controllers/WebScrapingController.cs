using API.Models;
using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using HtmlAgilityPack;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using System.Linq;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WebScrapingController : ControllerBase
    {
        private readonly IGraphClient _client;
        private readonly string _base_url = "https://www.wikitravel.org/en/";

        public WebScrapingController(IGraphClient client)
        {
            _client = client;
        }

        [HttpPost]
        [Route("scrape")]
        public async Task<IActionResult> ScrapeCity([FromBody] City city)
        {
            var url = _base_url + city.city_name;
            HttpClient client = new HttpClient();
            var response = await client.GetAsync(url);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound($"City {city.city_name} not found.");
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(responseBody);

            var seeSectionContent = GetSectionContent(htmlDoc, "//*[@id=\"See\"]");
            var doSectionContent = GetSectionContent(htmlDoc, "//*[@id=\"Do\"]");

            // Ensure the total token count is less than 1000
            (seeSectionContent, doSectionContent) = EnsureTokenLimit(city.city_name, seeSectionContent, doSectionContent);

            await SaveCitySectionsToDb(city.city_name, seeSectionContent, doSectionContent);

            return Ok(new { City = city.city_name, See = seeSectionContent, Do = doSectionContent });
        }

        private async Task SaveCitySectionsToDb(string cityName, string seeContent, string doContent)
        {
            await _client.Cypher
                .Match("(c:City {city_name: $cityName})")
                .Set("c.see_text = $seeContent, c.do_text = $doContent")
                .WithParams(new { cityName, seeContent, doContent })
                .ExecuteWithoutResultsAsync();
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
            int tokenLimit = 1000;

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
    }
}
