using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// s ovog githuba https://github.com/seble-nigussie/Neo4j-NET-core/blob/main/Controllers/DepartmentController.cs

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EmployeeController : ControllerBase
    {
        
            private readonly IGraphClient _client;

            public EmployeeController(IGraphClient client)
            {
                _client = client;
            }

            [HttpPost]
            public async Task<IActionResult> CreateEmployee([FromBody] Employee emp)
            {
                await _client.Cypher.Create("(e:Employee $emp)")
                                    .WithParam("emp", emp)
                                    .ExecuteWithoutResultsAsync();

                return Ok();
            }
    }

    public class Employee
    {
       public int id { get; set; }
       public string name { get; set; }
    }
}