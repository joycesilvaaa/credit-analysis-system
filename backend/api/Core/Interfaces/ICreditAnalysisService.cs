namespace backend.api.Service;

using backend.shared.Entities;
using backend.shared.Dtos;

public interface ICreditAnalysisService
{
    Task <CreditAnalysis> CreateAsync(CreditAnalysisDto dto);
    Task <CreditAnalysis?> GetByIdAsync(int id);
    Task <IEnumerable<CreditAnalysis>> GetAllAsync();
    Task Delete(int id);
}
