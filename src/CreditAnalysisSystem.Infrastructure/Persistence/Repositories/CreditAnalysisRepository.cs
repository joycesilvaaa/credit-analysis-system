using CreditAnalysisSystem.Application.Abstractions;
using CreditAnalysisSystem.Application.Exceptions;
using CreditAnalysisSystem.Domain.Entities;
using CreditAnalysisSystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CreditAnalysisSystem.Infrastructure.Persistence.Repositories;

internal sealed class CreditAnalysisRepository(AppDbContext context) : ICreditAnalysisRepository
{
    public Task<CreditAnalysis?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        context.CreditAnalyses.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<CreditAnalysis>> ListAsync(CreditAnalysisStatus? status, CancellationToken cancellationToken = default)
    {
        var query = context.CreditAnalyses.AsNoTracking();

        if (status is not null)
            query = query.Where(x => x.Status == status);

        return await query.OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<int>> GetPendingIdsUpdatedBeforeAsync(DateTime updatedBefore, int limit, CancellationToken cancellationToken = default) =>
        await context.CreditAnalyses
            .Where(x => x.Status == CreditAnalysisStatus.Pending && x.UpdatedAt < updatedBefore)
            .OrderBy(x => x.UpdatedAt)
            .Select(x => x.Id)
            .Take(limit)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(CreditAnalysis analysis, CancellationToken cancellationToken = default) =>
        await context.CreditAnalyses.AddAsync(analysis, cancellationToken);

    public void Remove(CreditAnalysis analysis) => context.CreditAnalyses.Remove(analysis);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyConflictException(
                "O registro foi alterado por outro processo. Recarregue e tente novamente.", ex);
        }
    }
}
