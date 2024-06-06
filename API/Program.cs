using Neo4jClient;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

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

builder.Services.AddSingleton<IMessageService<Message>, RabbitMQMessageService<Message>>(provider => 
{
    var factory = new ConnectionFactory
    {
        UserName = "guest",
        Password = "guest",
        VirtualHost = "/",
        HostName = "rabbitmq",
        Port = 5672,
    };

    var connection = factory.CreateConnection();
    var channel = connection.CreateModel();

    return new RabbitMQMessageService<Message>(channel);
});

builder.Services.AddHttpClient();

builder.Services.AddSingleton<RabbitMQConsumer>(provider =>
{
    var factory = new ConnectionFactory
    {
        UserName = "guest",
        Password = "guest",
        VirtualHost = "/",
        HostName = "rabbitmq",
        Port = 5672,
    };

    var connection = factory.CreateConnection();
    var channel = connection.CreateModel();

    var httpClient = provider.GetRequiredService<HttpClient>();
    var graphClient = provider.GetRequiredService<IGraphClient>();

    return new RabbitMQConsumer(channel, httpClient, graphClient);
});

builder.Services.AddSingleton<TouristGuideService>(provider =>
{
    var graphClient = provider.GetRequiredService<IGraphClient>();
    var httpClient = provider.GetRequiredService<HttpClient>();
    var configuration = provider.GetRequiredService<IConfiguration>();
    var apiToken = configuration["HuggingFace:ApiToken"];
    
    return new TouristGuideService(graphClient, httpClient, apiToken);
});

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
