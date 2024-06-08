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
            // _channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
            // _channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
}