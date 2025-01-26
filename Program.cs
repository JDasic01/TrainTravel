using Neo4jClient;
using System.Text;
using System.Text.Json;
using API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Register your services
builder.Services.AddSingleton<Neo4jService>(provider =>
{
    var neo4jSettings = builder.Configuration.GetSection("Neo4jSettings").Get<Neo4jSettings>();
    return new Neo4jService(neo4jSettings.Uri, neo4jSettings.Username, neo4jSettings.Password);
});

builder.Services.AddSingleton<IGraphClient>(provider =>
{
    var neo4jSettings = builder.Configuration.GetSection("Neo4jSettings").Get<Neo4jSettings>();
    var neo4jService = provider.GetRequiredService<Neo4jService>();
    var client = new BoltGraphClient(new Uri(neo4jSettings.Uri), neo4jSettings.Username, neo4jSettings.Password);
    client.ConnectAsync().Wait(); 
    return client;
});

builder.Services.AddHttpClient();

builder.Services.AddSingleton<TouristGuideService>(provider =>
{
    var graphClient = provider.GetRequiredService<IGraphClient>();
    var httpClient = provider.GetRequiredService<HttpClient>();
    var configuration = provider.GetRequiredService<IConfiguration>();
    var apiToken = configuration["HuggingFace:ApiToken"];
    
    return new TouristGuideService(graphClient, httpClient);
});

builder.Services.AddSingleton<WebScrapingService>(provider =>
{
    var graphClient = provider.GetRequiredService<IGraphClient>();
    var httpClient = provider.GetRequiredService<HttpClient>();
    var configuration = provider.GetRequiredService<IConfiguration>();
    var apiToken = configuration["HuggingFace:ApiToken"];
    
    return new WebScrapingService(graphClient, httpClient, apiToken);
});

builder.Services.AddSingleton<IHostedService, ScheduledHostedService>();

builder.Services.AddMemoryCache();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
