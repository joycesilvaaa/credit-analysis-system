using CreditAnalysisSystem.Application.Abstractions;
using CreditAnalysisSystem.Application.Messages;

namespace CreditAnalysisSystem.UnitTests.Fakes;

internal sealed class FakeCreditAnalysisPublisher : ICreditAnalysisPublisher
{
    public List<CreditAnalysisRequested> Published { get; } = [];
    public bool Fail { get; set; }

    public Task PublishAsync(CreditAnalysisRequested message, CancellationToken cancellationToken = default)
    {
        if (Fail)
            throw new InvalidOperationException("RabbitMQ fora do ar");
        Published.Add(message);
        return Task.CompletedTask;
    }
}
