using CreditAnalysisSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CreditAnalysisSystem.Infrastructure.Persistence;

internal sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<CreditAnalysis> CreditAnalyses => Set<CreditAnalysis>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
}
