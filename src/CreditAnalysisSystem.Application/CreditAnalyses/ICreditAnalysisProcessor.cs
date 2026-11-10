namespace CreditAnalysisSystem.Application.CreditAnalyses;

/// <summary>Avaliação de uma análise pendente (usado pelo worker).</summary>
public interface ICreditAnalysisProcessor
{
    Task ProcessAsync(int analysisId, CancellationToken cancellationToken = default);
}
