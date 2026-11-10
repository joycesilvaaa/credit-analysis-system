using CreditAnalysisSystem.Application.Abstractions;
using CreditAnalysisSystem.Application.Exceptions;
using CreditAnalysisSystem.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace CreditAnalysisSystem.Application.CreditAnalyses;

internal sealed class CreditAnalysisProcessor(
    ICreditAnalysisRepository repository,
    ILogger<CreditAnalysisProcessor> logger) : ICreditAnalysisProcessor
{
    public async Task ProcessAsync(int analysisId, CancellationToken cancellationToken = default)
    {
        var analysis = await repository.GetByIdAsync(analysisId, cancellationToken);

        if (analysis is null)
        {
            logger.LogWarning("Análise {AnalysisId} não existe mais. Mensagem ignorada.", analysisId);
            return;
        }

        // Idempotência: mensagens duplicadas ou já tratadas pela recuperação são ignoradas
        if (analysis.Status != CreditAnalysisStatus.Pending)
        {
            logger.LogInformation("Análise {AnalysisId} já avaliada ({Status}). Ignorada.", analysisId, analysis.Status);
            return;
        }

        analysis.Evaluate();

        try
        {
            await repository.SaveChangesAsync(cancellationToken);
        }
        catch (ConcurrencyConflictException)
        {
            // Outro processo alterou a análise (ex.: PUT ou outro worker); a mensagem mais recente prevalece
            logger.LogInformation("Análise {AnalysisId} foi alterada por outro processo. Avaliação descartada.", analysisId);
            return;
        }

        logger.LogInformation("Análise {AnalysisId} avaliada: {Status}.", analysisId, analysis.Status);
    }
}
