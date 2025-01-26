using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using API.Models;
using CsvHelper;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Newtonsoft.Json;


namespace API.Services;
public class RabbitMQMessageService<T> : IMessageService<T>, IDisposable
{
    private readonly IModel _channel;

    public RabbitMQMessageService(IModel channel)
    {
        _channel = channel;
    }

    public async Task SendMessageAsync(T message, string queueName)
    {
        var serializedMessage = JsonConvert.SerializeObject(message);
        var body = Encoding.UTF8.GetBytes(serializedMessage);

        await Task.Run(() => _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body));
    }

    public async Task<T> ReceiveMessageAsync(string queueName)
    {
        var result = await Task.Run(() =>
        {
            var messageResult = _channel.BasicGet(queueName, autoAck: true);
            if (messageResult == null)
                return default(T);

            var messageBody = Encoding.UTF8.GetString(messageResult.Body.ToArray());
            return JsonConvert.DeserializeObject<T>(messageBody);
        });

        return result;
    }

    public void Dispose()
    {
        _channel.Close();
        _channel.Dispose();
    }
}
