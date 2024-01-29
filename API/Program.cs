using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Neo4jClient;
using Neo4j.Driver;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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

builder.Services.AddHttpClient();

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

    return new RabbitMQMessageService<Message>(factory, connection, channel);
});

builder.Services.AddHostedService<RabbitMQConsumer>();
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddMemoryCache();
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

class RabbitMQConsumer : IHostedService
{
    private readonly ConnectionFactory _factory;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly HttpClient _httpClient;

    public RabbitMQConsumer(HttpClient httpClient)
    {
        _factory = new ConnectionFactory
        {
            UserName = "guest",
            Password = "guest",
            VirtualHost = "/",
            HostName = "rabbitmq",
            Port = 5672,
        };

        _connection = _factory.CreateConnection();
        _channel = _connection.CreateModel();
        _httpClient = httpClient;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _channel.QueueDeclare(queue: "line_queue", durable: false, exclusive: false, autoDelete: false, arguments: null);
        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            Console.WriteLine($"Received message: {message}");

            var receivedMessage = JsonSerializer.Deserialize<Message>(message);

            var response = await _httpClient.GetAsync($"http://api:8082/shortest-path?startCityId={receivedMessage.start_city_id}&endCityId={receivedMessage.end_city_id}");
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Shortest path result: {result}");
            }
            else
            {
                Console.WriteLine($"Error making GET request: {response.StatusCode}");
            }
        };

        _channel.BasicConsume(queue: "line_queue", autoAck: true, consumer: consumer);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _connection.Close();
        return Task.CompletedTask;
    }
}
