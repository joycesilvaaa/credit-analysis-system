using System.Reflection;
using CreditAnalysisSystem.Application;
using CreditAnalysisSystem.Domain.Entities;

namespace CreditAnalysisSystem.UnitTests.Architecture;

/// <summary>
/// Garante a regra de dependência da Clean Architecture:
/// as dependências apontam sempre para dentro (Infrastructure → Application → Domain).
/// </summary>
public class DependencyRuleTests
{
    private static readonly Assembly DomainAssembly = typeof(CreditAnalysis).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(DependencyInjection).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(CreditAnalysisSystem.Infrastructure.DependencyInjection).Assembly;

    private static readonly string[] ExternalTechnologies =
    [
        "Microsoft.EntityFrameworkCore",
        "Npgsql",
        "RabbitMQ.Client",
        "Microsoft.AspNetCore"
    ];

    [Fact]
    public void Domain_does_not_depend_on_any_other_layer_or_technology()
    {
        AssertDoesNotReference(DomainAssembly,
            [ApplicationAssembly.GetName().Name!, InfrastructureAssembly.GetName().Name!, "CreditAnalysisSystem.Api", "CreditAnalysisSystem.Worker",
             .. ExternalTechnologies, "Microsoft.Extensions"]);
    }

    [Fact]
    public void Application_depends_only_on_domain()
    {
        AssertDoesNotReference(ApplicationAssembly,
            [InfrastructureAssembly.GetName().Name!, "CreditAnalysisSystem.Api", "CreditAnalysisSystem.Worker", .. ExternalTechnologies]);
    }

    [Fact]
    public void Infrastructure_does_not_depend_on_presentation()
    {
        AssertDoesNotReference(InfrastructureAssembly, ["CreditAnalysisSystem.Api", "CreditAnalysisSystem.Worker", "Microsoft.AspNetCore"]);
    }

    [Fact]
    public void Infrastructure_exposes_only_the_dependency_injection_entry_point()
    {
        // Migrations são geradas pelo EF Core como públicas e ficam de fora
        var publicTypes = InfrastructureAssembly.GetExportedTypes()
            .Where(t => t.Namespace?.EndsWith(".Migrations", StringComparison.Ordinal) != true)
            .Select(t => t.Name);

        Assert.Equal(["DependencyInjection"], publicTypes);
    }

    private static void AssertDoesNotReference(Assembly assembly, IEnumerable<string> forbiddenPrefixes)
    {
        var references = assembly.GetReferencedAssemblies().Select(a => a.Name!).ToList();

        var violations = references
            .Where(reference => forbiddenPrefixes.Any(prefix => reference.StartsWith(prefix, StringComparison.Ordinal)))
            .ToList();

        Assert.True(violations.Count == 0,
            $"{assembly.GetName().Name} não deveria depender de: {string.Join(", ", violations)}");
    }
}
