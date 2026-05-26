using backend.api.Data;
using backend.api.Service;
using backend.api.Config;
using backend.api.Messaging;
using RabbitMQ.Client;
using Microsoft.EntityFrameworkCore;
using backend.api.Interfaces;


var builder = WebApplication.CreateBuilder(args);
DotNetEnv.Env.Load();
var connectionString = Environment.GetEnvironmentVariable("CONFIG_DB");
var rabbitHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST");
var factory = new ConnectionFactory { HostName = rabbitHost };

var connection = await factory.CreateConnectionAsync();
builder.Services.AddSingleton<IConnection>(connection);
builder.Services.AddDbContext<DataBaseConnection>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ICreditAnalysisRepository, CreditAnalysisRepository>();
builder.Services.AddScoped<ICreditAnalysisService, CreditAnalysisService>();
builder.Services.AddScoped<IRabbitMqPublisher, RabbitMqPublisher>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
