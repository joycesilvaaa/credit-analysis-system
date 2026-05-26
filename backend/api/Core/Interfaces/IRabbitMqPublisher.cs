namespace backend.api.Messaging;

public interface IRabbitMqPublisher
{
    Task Publish(string message);
}