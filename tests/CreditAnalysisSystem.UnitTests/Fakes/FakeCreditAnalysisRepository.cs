using System.Reflection;
using CreditAnalysisSystem.Application.Abstractions;
using CreditAnalysisSystem.Application.Exceptions;
using CreditAnalysisSystem.Domain.Entities;
using CreditAnalysisSystem.Domain.Enums;

namespace CreditAnalysisSystem.UnitTests.Fakes;

internal sealed class FakeCreditAnalysisRepository : ICreditAnalysisRepository
{
    private static readonly PropertyInfo IdProperty = typeof(CreditAnalysis).GetProperty(nameof(CreditAnalysis.Id))!;
    private readonly List<CreditAnalysis> _items = [];
    private int _nextId = 1;

    public int SaveCount { get; private set; }
    public bool ThrowConcurrencyOnSave { get; set; }
    public IReadOnlyList<CreditAnalysis> Items => _items;

    public CreditAnalysis Seed(CreditAnalysis analysis)
    {
        IdProperty.SetValue(analysis, _nextId++);
        _items.Add(analysis);
        return analysis;
    }

    public Task<CreditAnalysis?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_items.FirstOrDefault(x => x.Id == id));

    public Task<IReadOnlyList<CreditAnalysis>> ListAsync(CreditAnalysisStatus? status, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<CreditAnalysis>>(_items.Where(x => status is null || x.Status == status).ToList());

    public Task<IReadOnlyList<int>> GetPendingIdsUpdatedBeforeAsync(DateTime updatedBefore, int limit, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<int>>(_items
            .Where(x => x.Status == CreditAnalysisStatus.Pending && x.UpdatedAt < updatedBefore)
            .Select(x => x.Id).Take(limit).ToList());

    public Task AddAsync(CreditAnalysis analysis, CancellationToken cancellationToken = default)
    {
        Seed(analysis);
        return Task.CompletedTask;
    }

    public void Remove(CreditAnalysis analysis) => _items.Remove(analysis);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (ThrowConcurrencyOnSave)
            throw new ConcurrencyConflictException("conflito");
        SaveCount++;
        return Task.CompletedTask;
    }
}
