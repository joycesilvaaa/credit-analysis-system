using Microsoft.EntityFrameworkCore;
using backend.shared.Entities;

namespace backend.api.Config;

public class DataBaseConnection: DbContext
{
    public DataBaseConnection(DbContextOptions<DbContext> options) : base(options)
    {}
    public DbSet<CreditAnalysis> CreditAnalysis => Set<CreditAnalysis>();
}



