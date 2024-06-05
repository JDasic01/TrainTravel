using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using API.Models;
using Neo4jClient;

class RabbitMQConsumer : IHostedService
{
    private readonly ConnectionFactory _factory;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly HttpClient _httpClient;
    private readonly IGraphClient _graphClient;
    private readonly string _queueName = "cityQueue";

    public RabbitMQConsumer(HttpClient httpClient, IGraphClient graphClient)
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
        _graphClient = graphClient;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
        
        var cities = await _graphClient
            .Cypher.Match("(n:City)")
            .Return(n => n.As<City>())
            .ResultsAsync;

        foreach (var city in cities)
        {
            var message = JsonSerializer.Serialize(city);
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(exchange: "", routingKey: _queueName, basicProperties: null, body: body);
            Console.WriteLine($"Sent message for city: {city.city_name}");
        }

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var receivedCity = JsonSerializer.Deserialize<City>(message);

            var response = await _httpClient.PostAsJsonAsync("http://localhost:8082/WebScraping/scrape", receivedCity);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response for city {receivedCity.city_name}: {result}");
            }
            else
            {
                Console.WriteLine($"Error scraping city {receivedCity.city_name}: {response.StatusCode}");
            }
        };

        _channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _connection.Close();
        return Task.CompletedTask;
    }
}
