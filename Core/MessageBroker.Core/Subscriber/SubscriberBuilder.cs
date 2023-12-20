using MessageBroker.Core.MessageBroker;

namespace MessageBroker.Core.Subscriber;

public class SubscriberBuilder<T>
{
    public Type MessageType { get; }

    public MessageBrokerSettings Settings { get; }

    public SubscriberBuilder(MessageBrokerSettings settings)
        : this(settings, typeof(T))
    {
    }

    public SubscriberBuilder(MessageBrokerSettings settings, Type messageType)
    {
        MessageType = messageType;
        Settings = settings;
    }
}