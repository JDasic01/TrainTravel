using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using API.Models;
using API.Services;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Neo4jClient;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

public interface IMessageService<T>
{
    Task SendMessageAsync(T message, string channel);
    Task<T> ReceiveMessageAsync(string channel);
}

public class RabbitMQMessageService<T> : IMessageService<T>, IDisposable
{
    private readonly ConnectionFactory _factory;
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMQMessageService(ConnectionFactory factory, IConnection connection, IModel channel)
    {
        _factory = factory;
        _connection = _factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(queue: "lines_queue", durable: false, exclusive: false, autoDelete: false, arguments: null);
    }

    public async Task SendMessageAsync(T message, string channel)
    {
        var serializedMessage = Newtonsoft.Json.JsonConvert.SerializeObject(message);
        var body = Encoding.UTF8.GetBytes(serializedMessage);

        await Task.Run(() => _channel.BasicPublish(exchange: "", routingKey: channel, basicProperties: null, body: body));
    }

    public async Task<T> ReceiveMessageAsync(string channel)
    {
        var result = await Task.Run(() =>
        {
            BasicGetResult messageResult = _channel.BasicGet(channel, autoAck: true);
            if (messageResult == null)
                return default(T);

            var messageBody = Encoding.UTF8.GetString(messageResult.Body.ToArray());
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(messageBody);
        });

        return result;
    }

    public void Dispose()
    {
        _channel.Close();
        _connection.Close();
    }
}
