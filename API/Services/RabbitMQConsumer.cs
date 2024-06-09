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

public class RabbitMQConsumer : IHostedService
{
    private readonly IModel _channel;
    private readonly HttpClient _httpClient;
    private readonly IGraphClient _graphClient;
    private readonly string _queueName = "cityQueue";

    public RabbitMQConsumer(IModel channel, HttpClient httpClient, IGraphClient graphClient)
    {
        _channel = channel;
        _httpClient = httpClient;
        _graphClient = graphClient;
    }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            var cities = await _graphClient
                .Cypher.Match("(n:City)")
                .Where("exists(n.see) AND exists(n.do)")
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

                // Prepare payload to send to external API
                var payload = new
                {
                    cityName = receivedCity.city_name,
                    text = $"see: {receivedCity.see_text}\ndo: {receivedCity.do_text}"
                };

                var response = await _httpClient.PostAsJsonAsync("http://api:8082/WebScraping/scrape", payload);

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
            return Task.CompletedTask;
        }

        public Task StartWebscraping()
        {
            // send messages to webscraping queue, do it for cities that dont have see and do in db
            return Task.CompletedTask;
        }

        public Task StartTouristGuide()
        {
            // send messages to tourist queue, do it for cities that dont have touristGuide text in db
            return Task.CompletedTask;
        }

        public Task StartTranslations()
        {
            // send messages to translations queue, do it for cities that dont have translations text in db
            return Task.CompletedTask;
        }

        public Task StartVoiceGeneration()
        {
            // send messages to polly? queue, do it for cities that dont have audio files for all languages
            return Task.CompletedTask;
        }
}