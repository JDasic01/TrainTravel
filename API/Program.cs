using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Neo4jClient;
using Neo4j.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddSingleton<Neo4jService>(provider =>
{
    var neo4jSettings = builder.Configuration.GetSection("Neo4jSettings").Get<Neo4jSettings>();
    return new Neo4jService(neo4jSettings.Uri, neo4jSettings.Username, neo4jSettings.Password);
});

// Use Neo4jService to create the GraphClient and connect to Neo4j
builder.Services.AddSingleton<IGraphClient>(provider =>
{
    var neo4jSettings = builder.Configuration.GetSection("Neo4jSettings").Get<Neo4jSettings>();
    var neo4jService = provider.GetRequiredService<Neo4jService>();
    var client = new BoltGraphClient(new Uri(neo4jSettings.Uri), neo4jSettings.Username, neo4jSettings.Password);
    client.ConnectAsync().Wait(); // Wait for the connection to complete
    return client;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
