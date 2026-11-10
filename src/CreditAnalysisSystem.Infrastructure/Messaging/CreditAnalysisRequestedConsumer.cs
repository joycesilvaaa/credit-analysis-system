using System.Text.Json;
using CreditAnalysisSystem.Application.CreditAnalyses;
using CreditAnalysisSystem.Application.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CreditAnalysisSystem.Infrastructure.Messaging;

/// <summary>Consome a fila de análises e dispara a avaliação de cada uma.</summary>
internal sealed class CreditAnalysisRequestedConsumer(
    RabbitMqConnectionProvider connectionProvider,
    IServiceScopeFactory scopeFactory,
    ILogger<CreditAnalysisRequestedConsumer> logger) : BackgroundService
{
    private const ushort PrefetchCount = 10;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connection = await connectionProvider.GetConnectionAsync(stoppingToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);
        await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: PrefetchCount, global: false, stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += (_, ea) => HandleAsync(channel, ea, stoppingToken);

        await channel.BasicConsumeAsync(RabbitMqTopology.CreditAnalysisQueue, autoAck: false, consumer, stoppingToken);
        logger.LogInformation("Consumindo a fila {Queue}.", RabbitMqTopology.CreditAnalysisQueue);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // Encerramento normal
        }
    }

    private async Task HandleAsync(IChannel channel, BasicDeliverEventArgs ea, CancellationToken stoppingToken)
    {
        CreditAnalysisRequested? message;
        try
        {
            message = JsonSerializer.Deserialize<CreditAnalysisRequested>(ea.Body.Span);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Mensagem inválida descartada para a DLQ.");
            await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
            return;
        }

        if (message is null || message.AnalysisId <= 0)
        {
            logger.LogError("Mensagem vazia ou sem AnalysisId descartada para a DLQ.");
            await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
            return;
        }

        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var processor = scope.ServiceProvider.GetRequiredService<ICreditAnalysisProcessor>();
            await processor.ProcessAsync(message.AnalysisId, stoppingToken);

            await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
            // Vai para a DLQ; a análise continua Pending no banco e será retomada pela rotina de recuperação
            logger.LogError(ex, "Erro ao processar a análise {AnalysisId}.", message.AnalysisId);
            await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
        }
    }
}
