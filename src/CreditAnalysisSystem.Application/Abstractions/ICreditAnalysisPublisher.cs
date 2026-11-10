using CreditAnalysisSystem.Application.Messages;

namespace CreditAnalysisSystem.Application.Abstractions;

public interface ICreditAnalysisPublisher
{
    Task PublishAsync(CreditAnalysisRequested message, CancellationToken cancellationToken = default);
}
