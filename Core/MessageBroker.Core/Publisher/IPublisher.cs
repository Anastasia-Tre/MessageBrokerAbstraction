namespace MessageBroker.Core.Publisher;

public interface IPublisher
{
    Task Publish<TMessage>(TMessage message, string name = null);
}