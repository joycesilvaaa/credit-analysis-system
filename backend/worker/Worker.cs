using backend.worker.Consumers;

namespace worker;

public class Worker(RabbitMqConsumer consumer) : BackgroundService
{

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        await consumer.ConsumeAsync(stoppingToken);
    }
}