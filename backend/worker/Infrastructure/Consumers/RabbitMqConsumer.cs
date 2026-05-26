using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using backend.worker.DTOs;
using backend.worker.Services;

namespace backend.worker.Consumers;

public class RabbitMqConsumer(IServiceScopeFactory scopeFactory, ILogger<RabbitMqConsumer> logger, IConnection connection)
{

    public async Task ConsumeAsync(CancellationToken cancellationToken)
    {
        var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: "credit-analysis",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(
                    ea.Body.ToArray());

                var message = JsonSerializer.Deserialize
                    <CreditAnalysisMessage>(json);

                if (message is null)
                    return;

                using var scope =
                    scopeFactory.CreateScope();

                var service = scope.ServiceProvider
                    .GetRequiredService<ICreditAnalysisService>();

                await service.ProcessAsync(message);

                logger.LogInformation(
                    "Message processed: {Id}",
                    message.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro");

                await channel.BasicNackAsync(
                    ea.DeliveryTag,
                    false,
                    true,
                    cancellationToken);
            }
        };

    await channel.BasicConsumeAsync(
        queue: "credit-analysis",
        autoAck: false,
        consumer: consumer,
        cancellationToken: cancellationToken);

    await Task.Delay(Timeout.Infinite,cancellationToken);
    }
}