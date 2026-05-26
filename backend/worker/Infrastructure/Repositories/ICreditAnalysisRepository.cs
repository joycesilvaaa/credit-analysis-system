namespace backend.worker.Repositories;

using backend.shared.Entities;


public interface ICreditAnalysisRepository
{
    Task<CreditAnalysis?> GetByIdAsync(int id);

    Task SaveChangesAsync();
}