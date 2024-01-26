using API.Models;
using RabbitMQ.Client;
using System.Text;

public interface IMessageService
{
    Task SendMessageAsync(City city);
}

public class RabbitMQMessageService : IMessageService
{
    private readonly ConnectionFactory _factory;

    public RabbitMQMessageService(ConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task SendMessageAsync(City city)
    {
        using (var connection = _factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.QueueDeclare(queue: "city_routes", durable: false, exclusive: false, autoDelete: false, arguments: null);

            var message = Newtonsoft.Json.JsonConvert.SerializeObject(city);
            var body = Encoding.UTF8.GetBytes(message);

            await Task.Run(() => channel.BasicPublish(exchange: "", routingKey: "city_routes", basicProperties: null, body: body));
        }
    }
}