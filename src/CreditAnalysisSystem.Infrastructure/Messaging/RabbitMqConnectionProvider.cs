using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace CreditAnalysisSystem.Infrastructure.Messaging;

/// <summary>
/// Mantém uma única conexão com o RabbitMQ, criada sob demanda com re-tentativas.
/// A topologia (filas/exchanges) é declarada na primeira conexão.
/// </summary>
internal sealed class RabbitMqConnectionProvider(
    IOptions<RabbitMqOptions> options,
    ILogger<RabbitMqConnectionProvider> logger) : IAsyncDisposable
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private IConnection? _connection;

    public async Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (_connection is not null)
            return _connection;

        await _lock.WaitAsync(cancellationToken);
        try
        {
            _connection ??= await ConnectAsync(cancellationToken);
            return _connection;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<IConnection> ConnectAsync(CancellationToken cancellationToken)
    {
        var settings = options.Value;
        var factory = new ConnectionFactory
        {
            HostName = settings.Host,
            Port = settings.Port,
            UserName = settings.UserName,
            Password = settings.Password,
            AutomaticRecoveryEnabled = true
        };

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                var connection = await factory.CreateConnectionAsync(cancellationToken);

                await using (var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken))
                    await RabbitMqTopology.DeclareAsync(channel, cancellationToken);

                logger.LogInformation("Conectado ao RabbitMQ em {Host}:{Port}.", settings.Host, settings.Port);
                return connection;
            }
            catch (Exception ex) when (attempt < settings.ConnectRetryCount && ex is not OperationCanceledException)
            {
                logger.LogWarning("RabbitMQ indisponível ({Message}). Tentativa {Attempt}/{Max}.",
                    ex.Message, attempt, settings.ConnectRetryCount);
                await Task.Delay(TimeSpan.FromSeconds(settings.ConnectRetryDelaySeconds), cancellationToken);
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
            await _connection.DisposeAsync();
        _lock.Dispose();
    }
}
