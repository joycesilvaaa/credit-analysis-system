using CreditAnalysisSystem.Application.Abstractions;
using CreditAnalysisSystem.Application.CreditAnalyses;
using Microsoft.Extensions.Options;

namespace CreditAnalysisSystem.Worker.Recovery;

/// <summary>
/// Rede de segurança: avalia análises que ficaram em Pending por muito tempo
/// (mensagem não publicada, worker fora do ar, erro no processamento...).
/// </summary>
public sealed class PendingAnalysisRecoveryWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<PendingRecoveryOptions> options,
    ILogger<PendingAnalysisRecoveryWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(settings.IntervalSeconds));

        try
        {
            do
            {
                await RecoverAsync(settings, stoppingToken);
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException)
        {
            // Encerramento normal
        }
    }

    private async Task RecoverAsync(PendingRecoveryOptions settings, CancellationToken stoppingToken)
    {
        IReadOnlyList<int> staleIds;
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<ICreditAnalysisRepository>();
            var updatedBefore = DateTime.UtcNow.AddSeconds(-settings.StaleAfterSeconds);
            staleIds = await repository.GetPendingIdsUpdatedBeforeAsync(updatedBefore, settings.BatchSize, stoppingToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Falha ao buscar análises pendentes.");
            return;
        }

        if (staleIds.Count == 0)
            return;

        logger.LogWarning("Recuperando {Count} análise(s) presa(s) em Pending.", staleIds.Count);

        foreach (var id in staleIds)
        {
            try
            {
                // Um escopo (DbContext) por análise para isolar falhas
                await using var scope = scopeFactory.CreateAsyncScope();
                var processor = scope.ServiceProvider.GetRequiredService<ICreditAnalysisProcessor>();
                await processor.ProcessAsync(id, stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Falha ao recuperar a análise {AnalysisId}.", id);
            }
        }
    }
}
