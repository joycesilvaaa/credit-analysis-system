using worker;
using backend.worker.Services;
using backend.worker.Config;
using backend.worker.Repositories;
using backend.worker.Consumers;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

var builder = Host.CreateApplicationBuilder(args);
var connectionString = Environment.GetEnvironmentVariable("CONFIG_DB");
builder.Services.AddHostedService<Worker>();
var factory = new ConnectionFactory { HostName = "rabbitmq" };
var connection = await factory.CreateConnectionAsync();
builder.Services.AddSingleton<IConnection>(connection);
builder.Services.AddScoped<RabbitMqConsumer>();
builder.Services.AddScoped<ICreditAnalysisRepository,CreditAnalysisRepository>();
builder.Services.AddScoped<ICreditAnalysisService,CreditAnalysisService>();
builder.Services.AddDbContext<DataBaseConnection>(options =>
    options.UseNpgsql(connectionString));
var host = builder.Build();
host.Run();
