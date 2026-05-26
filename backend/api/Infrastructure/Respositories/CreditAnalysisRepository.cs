namespace backend.api.Data;

using backend.shared.Entities;
using Microsoft.EntityFrameworkCore;
using backend.api.Config;
using backend.api.Interfaces;

public class CreditAnalysisRepository(DataBaseConnection context) : ICreditAnalysisRepository
{

    public async Task <CreditAnalysis?> GetByIdAsync(int id)
    {
        return await context.CreditAnalysis.FindAsync(id);
    }

    public async Task <IEnumerable<CreditAnalysis>> GetAllAsync()
    {
        return await context.CreditAnalysis.ToListAsync();
    }

    public async Task AddAsync(CreditAnalysis analysis)
    {
        await context.CreditAnalysis.AddAsync(analysis);
    }

    public async Task Delete(CreditAnalysis analysis)
    {
        context.CreditAnalysis.Remove(analysis);
    }

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }
}

