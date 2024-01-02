using MessageBroker.Core.Publisher;
using MessageBroker.Core.Subscriber;

namespace MessageBroker.Core.MessageBroker;

public interface IMessageBroker : IDisposable, IPublisher, ISubscriberControl
{
}
