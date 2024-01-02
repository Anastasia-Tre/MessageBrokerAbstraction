using MessageBroker.Core.Subscriber;

namespace MessageBroker.Core.Message;

public interface IMessageHandler<in TMessage>
{
    SubscriberSettings SubscriberSettings { get; }

    Task HandleMessage(TMessage transportMessage);
}
