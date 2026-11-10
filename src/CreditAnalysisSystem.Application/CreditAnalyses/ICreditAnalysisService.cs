using CreditAnalysisSystem.Domain.Enums;

namespace CreditAnalysisSystem.Application.CreditAnalyses;

/// <summary>Casos de uso expostos pela API.</summary>
public interface ICreditAnalysisService
{
    Task<CreditAnalysisResponse> CreateAsync(CreditAnalysisRequest request, CancellationToken cancellationToken = default);
    Task<CreditAnalysisResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CreditAnalysisResponse>> ListAsync(CreditAnalysisStatus? status, CancellationToken cancellationToken = default);
    Task<CreditAnalysisResponse> UpdateAsync(int id, CreditAnalysisRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
