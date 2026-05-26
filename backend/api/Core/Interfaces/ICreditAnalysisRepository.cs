namespace backend.api.Interfaces;

using backend.shared.Entities;


public interface ICreditAnalysisRepository
{
    Task <CreditAnalysis?> GetByIdAsync(int id);
    Task <IEnumerable<CreditAnalysis>> GetAllAsync();
    Task AddAsync(CreditAnalysis analysis);
    Task Delete(CreditAnalysis analysis);
    Task SaveChangesAsync();
}

