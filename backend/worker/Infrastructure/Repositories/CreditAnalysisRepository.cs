using Microsoft.EntityFrameworkCore;
using backend.shared.Entities;
using backend.worker.Config;

namespace backend.worker.Repositories;

public class CreditAnalysisRepository(DataBaseConnection connection) : ICreditAnalysisRepository
{

    public async Task<CreditAnalysis?> GetByIdAsync(int id)
    {
        return await connection.CreditAnalysis
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task SaveChangesAsync()
    {
        await connection.SaveChangesAsync();
    }
}