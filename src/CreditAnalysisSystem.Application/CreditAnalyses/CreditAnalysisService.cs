using CreditAnalysisSystem.Application.Abstractions;
using CreditAnalysisSystem.Application.Exceptions;
using CreditAnalysisSystem.Application.Messages;
using CreditAnalysisSystem.Domain.Entities;
using CreditAnalysisSystem.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace CreditAnalysisSystem.Application.CreditAnalyses;

internal sealed class CreditAnalysisService(
    ICreditAnalysisRepository repository,
    ICreditAnalysisPublisher publisher,
    ILogger<CreditAnalysisService> logger) : ICreditAnalysisService
{
    public async Task<CreditAnalysisResponse> CreateAsync(CreditAnalysisRequest request, CancellationToken cancellationToken = default)
    {
        var analysis = new CreditAnalysis(request.Name, request.Income, request.Score);

        await repository.AddAsync(analysis, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        await RequestEvaluationAsync(analysis.Id, cancellationToken);

        return CreditAnalysisResponse.From(analysis);
    }

    public async Task<CreditAnalysisResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var analysis = await GetExistingAsync(id, cancellationToken);
        return CreditAnalysisResponse.From(analysis);
    }

    public async Task<IReadOnlyList<CreditAnalysisResponse>> ListAsync(CreditAnalysisStatus? status, CancellationToken cancellationToken = default)
    {
        var analyses = await repository.ListAsync(status, cancellationToken);
        return analyses.Select(CreditAnalysisResponse.From).ToList();
    }

    public async Task<CreditAnalysisResponse> UpdateAsync(int id, CreditAnalysisRequest request, CancellationToken cancellationToken = default)
    {
        var analysis = await GetExistingAsync(id, cancellationToken);

        analysis.Update(request.Name, request.Income, request.Score);
        await repository.SaveChangesAsync(cancellationToken);
        await RequestEvaluationAsync(analysis.Id, cancellationToken);

        return CreditAnalysisResponse.From(analysis);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var analysis = await GetExistingAsync(id, cancellationToken);

        if (!analysis.CanBeDeleted)
            throw new ConflictException($"A análise {id} ainda está pendente de avaliação e não pode ser removida.");

        repository.Remove(analysis);
        await repository.SaveChangesAsync(cancellationToken);
    }

    private async Task<CreditAnalysis> GetExistingAsync(int id, CancellationToken cancellationToken) =>
        await repository.GetByIdAsync(id, cancellationToken)
        ?? throw new NotFoundException($"Análise {id} não encontrada.");

    /// <summary>
    /// Publica a análise na fila. Se o RabbitMQ estiver fora, a análise já está salva
    /// como Pending e será avaliada pela rotina de recuperação do worker.
    /// </summary>
    private async Task RequestEvaluationAsync(int analysisId, CancellationToken cancellationToken)
    {
        try
        {
            await publisher.PublishAsync(new CreditAnalysisRequested(analysisId), cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex,
                "Falha ao publicar a análise {AnalysisId}. Ela será avaliada pela rotina de recuperação.", analysisId);
        }
    }
}
