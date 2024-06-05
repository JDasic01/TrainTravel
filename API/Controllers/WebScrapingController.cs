using API.Models;
using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using HtmlAgilityPack;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;

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

            await SaveCitySectionsToDb(city.city_name, seeSectionContent, doSectionContent);

            return Ok(new { City = city.city_name, See = seeSectionContent, Do = doSectionContent });
        }

        private async Task SaveCitySectionsToDb(string cityName, string seeContent, string doContent)
        {
            await _client.Cypher
                .Match("(c:City {city_name: $cityName})")
                .Set("c.see = $seeContent, c.do = $doContent")
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
    }
}
