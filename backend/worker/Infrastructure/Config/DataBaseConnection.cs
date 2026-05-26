using Microsoft.EntityFrameworkCore;
using backend.shared.Entities;

namespace backend.worker.Config;

public class DataBaseConnection: DbContext
{
    public DataBaseConnection(DbContextOptions<DbContext> options) : base(options)
    {}
    public DbSet<CreditAnalysis> CreditAnalysis => Set<CreditAnalysis>();
}



