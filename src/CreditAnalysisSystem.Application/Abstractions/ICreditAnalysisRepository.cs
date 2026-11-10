using CreditAnalysisSystem.Domain.Entities;
using CreditAnalysisSystem.Domain.Enums;

namespace CreditAnalysisSystem.Application.Abstractions;

public interface ICreditAnalysisRepository
{
    Task<CreditAnalysis?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CreditAnalysis>> ListAsync(CreditAnalysisStatus? status, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<int>> GetPendingIdsUpdatedBeforeAsync(DateTime updatedBefore, int limit, CancellationToken cancellationToken = default);
    Task AddAsync(CreditAnalysis analysis, CancellationToken cancellationToken = default);
    void Remove(CreditAnalysis analysis);

    /// <exception cref="Exceptions.ConcurrencyConflictException">Registro alterado por outro processo.</exception>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
