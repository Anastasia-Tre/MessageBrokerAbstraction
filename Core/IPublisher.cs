namespace MessageBroker.Core;

public interface IPublisher
{
    Task Publish<TMessage>(TMessage message);
}