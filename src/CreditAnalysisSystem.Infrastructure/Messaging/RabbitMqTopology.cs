using RabbitMQ.Client;

namespace CreditAnalysisSystem.Infrastructure.Messaging;

/// <summary>Filas e exchanges usadas pelo sistema. Declarar é idempotente.</summary>
internal static class RabbitMqTopology
{
    public const string CreditAnalysisQueue = "credit-analysis.requested";
    public const string DeadLetterExchange = "credit-analysis.dlx";
    public const string DeadLetterQueue = "credit-analysis.requested.dlq";

    public static async Task DeclareAsync(IChannel channel, CancellationToken cancellationToken = default)
    {
        await channel.ExchangeDeclareAsync(DeadLetterExchange, ExchangeType.Direct, durable: true,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(DeadLetterQueue, durable: true, exclusive: false, autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(DeadLetterQueue, DeadLetterExchange, routingKey: CreditAnalysisQueue,
            cancellationToken: cancellationToken);

        // Mensagens rejeitadas (nack sem requeue) vão para a DLQ
        await channel.QueueDeclareAsync(CreditAnalysisQueue, durable: true, exclusive: false, autoDelete: false,
            arguments: new Dictionary<string, object?> { ["x-dead-letter-exchange"] = DeadLetterExchange },
            cancellationToken: cancellationToken);
    }
}
