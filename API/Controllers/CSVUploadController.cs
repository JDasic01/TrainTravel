// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Extensions.Logging;
// using System;
// using System.IO;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Hosting;
// using Microsoft.EntityFrameworkCore;

// namespace API.Controllers
// {
//     [ApiController]
//     [Route("[controller]")]
//     public class CSVUploadController : ControllerBase
//     {
//         private readonly ILogger<CSVUploadController> _logger;
//         private readonly IWebHostEnvironment _webHostEnvironment;
//         private readonly YourDbContext _dbContext; // Replace YourDbContext with your actual DbContext type

//         public CSVUploadController(ILogger<CSVUploadController> logger, IWebHostEnvironment webHostEnvironment, YourDbContext dbContext)
//         {
//             _logger = logger;
//             _webHostEnvironment = webHostEnvironment;
//             _dbContext = dbContext;
//         }

//         [HttpPost(Name = "PostCSVFile")]
//         public async Task<ActionResult<string>> PostCsv([FromForm] UploadFile obj)
//         {
//             if (obj.file.Length > 0)
//             {
//                 try
//                 {
//                     // Save CSV file to the server
//                     string filePath = _webHostEnvironment.WebRootPath + "\\CSVFiles\\" + obj.file.FileName;

//                     using (FileStream fileStream = System.IO.File.Create(filePath))
//                     {
//                         obj.file.CopyTo(fileStream);
//                         fileStream.Flush();
//                     }

//                     // Read data from the CSV file
//                     var csvData = ReadCsvFile(filePath);

//                     // Process and save data to the database
//                     await ProcessAndSaveToDatabase(csvData);

//                     return "CSV data added to the database";
//                 }
//                 catch (Exception ex)
//                 {
//                     return ex.ToString();
//                 }
//             }
//             else
//             {
//                 return "Upload failed";
//             }
//         }

//         private List<YourCsvModel> ReadCsvFile(string filePath)
//         {
//             // Implement logic to read CSV file and return data as a list of your model
//             // You can use a CSV parsing library or implement custom logic based on your CSV format
//         }

//         private async Task ProcessAndSaveToDatabase(List<YourCsvModel> csvData)
//         {
//             foreach (var item in csvData)
//             {
//                 _dbContext.YourEntities.Add(item);
//             }

//             await _dbContext.SaveChangesAsync();
//         }
//     }
// }
