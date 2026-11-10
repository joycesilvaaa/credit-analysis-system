using CreditAnalysisSystem.Application.CreditAnalyses;
using Microsoft.Extensions.DependencyInjection;

namespace CreditAnalysisSystem.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICreditAnalysisService, CreditAnalysisService>();
        services.AddScoped<ICreditAnalysisProcessor, CreditAnalysisProcessor>();
        return services;
    }
}
