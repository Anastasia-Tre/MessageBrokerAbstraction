using MessageBroker.Core.Publisher;

namespace MessageBroker.Core.MessageBroker;

public interface IMessageBroker : IDisposable, IPublisher
{
}