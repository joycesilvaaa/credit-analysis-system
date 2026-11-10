using CreditAnalysisSystem.Application;
using CreditAnalysisSystem.Infrastructure;
using CreditAnalysisSystem.Worker.Recovery;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddMessageConsumers();

builder.Services
    .AddOptions<PendingRecoveryOptions>()
    .Bind(builder.Configuration.GetSection(PendingRecoveryOptions.SectionName));

builder.Services.AddHostedService<PendingAnalysisRecoveryWorker>();

builder.Build().Run();
