using System.Text.Json;
using CreditAnalysisSystem.Application.Abstractions;
using CreditAnalysisSystem.Application.Messages;
using RabbitMQ.Client;

namespace CreditAnalysisSystem.Infrastructure.Messaging;

internal sealed class RabbitMqCreditAnalysisPublisher(RabbitMqConnectionProvider connectionProvider)
    : ICreditAnalysisPublisher
{
    private static readonly CreateChannelOptions ChannelOptions = new(
        publisherConfirmationsEnabled: true,
        publisherConfirmationTrackingEnabled: true);

    public async Task PublishAsync(CreditAnalysisRequested message, CancellationToken cancellationToken = default)
    {
        var connection = await connectionProvider.GetConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(ChannelOptions, cancellationToken);

        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            Type = nameof(CreditAnalysisRequested)
        };

        // Com publisher confirms, lança exceção se o broker não confirmar o recebimento
        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: RabbitMqTopology.CreditAnalysisQueue,
            mandatory: true,
            basicProperties: properties,
            body: JsonSerializer.SerializeToUtf8Bytes(message),
            cancellationToken: cancellationToken);
    }
}
