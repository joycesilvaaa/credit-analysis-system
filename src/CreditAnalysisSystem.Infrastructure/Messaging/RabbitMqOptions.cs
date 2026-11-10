namespace CreditAnalysisSystem.Infrastructure.Messaging;

internal sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string Host { get; init; } = "localhost";
    public int Port { get; init; } = 5672;
    public string UserName { get; init; } = "guest";
    public string Password { get; init; } = "guest";
    public int ConnectRetryCount { get; init; } = 10;
    public int ConnectRetryDelaySeconds { get; init; } = 5;
}
