using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CreditAnalysisSystem.Infrastructure.Persistence;

/// <summary>Usado apenas pelo `dotnet ef` para gerar migrations.</summary>
internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Port=5433;Database=credit_analysis;Username=postgres;Password=postgres")
            .Options;
        return new AppDbContext(options);
    }
}
