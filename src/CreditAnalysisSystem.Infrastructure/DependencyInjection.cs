using CreditAnalysisSystem.Application.Abstractions;
using CreditAnalysisSystem.Infrastructure.Messaging;
using CreditAnalysisSystem.Infrastructure.Persistence;
using CreditAnalysisSystem.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CreditAnalysisSystem.Infrastructure;

/// <summary>
/// Único ponto de contato das camadas externas (Api/Worker) com a Infrastructure.
/// Os detalhes (EF Core, RabbitMQ) ficam internos a este projeto.
/// </summary>
public static class DependencyInjection
{
    /// <summary>Banco de dados, repositórios e publicação de mensagens.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' não configurada.");

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<ICreditAnalysisRepository, CreditAnalysisRepository>();

        services.AddOptions<RabbitMqOptions>().Bind(configuration.GetSection(RabbitMqOptions.SectionName));
        services.AddSingleton<RabbitMqConnectionProvider>();
        services.AddScoped<ICreditAnalysisPublisher, RabbitMqCreditAnalysisPublisher>();

        services.AddHealthChecks().AddDbContextCheck<AppDbContext>("database");

        return services;
    }

    /// <summary>Consumo da fila de análises (usado pelo Worker).</summary>
    public static IServiceCollection AddMessageConsumers(this IServiceCollection services)
    {
        services.AddHostedService<CreditAnalysisRequestedConsumer>();
        return services;
    }

    /// <summary>Aplica as migrations e abre a conexão com o RabbitMQ (com re-tentativas).</summary>
    public static async Task InitializeInfrastructureAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        await using (var scope = serviceProvider.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync(cancellationToken);
        }

        await serviceProvider.GetRequiredService<RabbitMqConnectionProvider>().GetConnectionAsync(cancellationToken);
    }
}
