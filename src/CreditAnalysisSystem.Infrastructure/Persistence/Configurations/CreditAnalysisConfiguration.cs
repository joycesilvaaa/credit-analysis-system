using CreditAnalysisSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CreditAnalysisSystem.Infrastructure.Persistence.Configurations;

internal sealed class CreditAnalysisConfiguration : IEntityTypeConfiguration<CreditAnalysis>
{
    public void Configure(EntityTypeBuilder<CreditAnalysis> builder)
    {
        builder.ToTable("credit_analyses");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(CreditAnalysis.NameMaxLength).IsRequired();
        builder.Property(x => x.Income).HasPrecision(18, 2);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.RejectionReason).HasMaxLength(200);

        // Concorrência otimista via coluna de sistema xmin do Postgres.
        // Shadow property: detalhe de persistência que não aparece na entidade de domínio.
        builder.Property<uint>("Version").IsRowVersion();

        builder.HasIndex(x => new { x.Status, x.UpdatedAt });
    }
}
