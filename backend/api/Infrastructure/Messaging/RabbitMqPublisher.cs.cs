using System.Text;
using RabbitMQ.Client;

namespace backend.api.Messaging;

public class RabbitMqPublisher(IConnection connection) : IRabbitMqPublisher
{

    public async Task Publish(string message) 
    {
        using var channel = await connection.CreateChannelAsync(); 

        var body = Encoding.UTF8.GetBytes(message);

        await channel.QueueDeclareAsync(
            queue: "credit-analysis",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: "credit-analysis",
            body: body
        );
    }
}