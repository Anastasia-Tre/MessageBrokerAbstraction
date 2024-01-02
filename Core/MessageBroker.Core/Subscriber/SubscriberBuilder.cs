using MessageBroker.Core.MessageBroker;

namespace MessageBroker.Core.Subscriber;

public class SubscriberBuilder<T>
{
    public SubscriberSettings SubscriberSettings;

    public SubscriberBuilder(MessageBrokerSettings settings, Type messageType,
        string? topic = null)
    {
        Settings = settings ??
                   throw new ArgumentNullException(nameof(settings));

        SubscriberSettings = new SubscriberSettings
        {
            MessageType = messageType,
            Topic = topic
        };
        Settings.Subscribers.Add(SubscriberSettings);
    }

    public SubscriberBuilder(MessageBrokerSettings settings)
        : this(settings, typeof(T))
    {
    }

    public MessageBrokerSettings Settings { get; }

    public SubscriberBuilder<T> Topic(string? topic)
    {
        SubscriberSettings.Topic = topic;
        return this;
    }

    public SubscriberBuilder<T> WithSubscriber<TSubscriber>()
        where TSubscriber : class, ISubscriber<T>
    {
        SubscriberSettings.SubscriberType = typeof(TSubscriber);
        SubscriberSettings.SubscriberMethod = (subscriber, message) =>
            ((ISubscriber<T>)subscriber).OnHandle((T)message);

        SubscriberSettings.Invokers.Add(SubscriberSettings);

        return this;
    }

    public SubscriberBuilder<T> WithSubscriber<TSubscriber, TMessage>()
        where TSubscriber : class, ISubscriber<TMessage>
        where TMessage : T
    {
        var invoker = new SubscriberSettings
        {
            SubscriberMethod = (subscriber, message) =>
                ((ISubscriber<TMessage>)subscriber).OnHandle((TMessage)message)
        };
        SubscriberSettings.Invokers.Add(invoker);

        return this;
    }

    public SubscriberBuilder<T> WithSubscriber<TSubscriber>(
        Func<TSubscriber, T, Task> subscriberMethod)
        where TSubscriber : class
    {
        ArgumentNullException.ThrowIfNull(subscriberMethod);

        SubscriberSettings.SubscriberType = typeof(TSubscriber);
        SubscriberSettings.SubscriberMethod = (subscriber, message) =>
            subscriberMethod((TSubscriber)subscriber, (T)message);

        SubscriberSettings.Invokers.Add(SubscriberSettings);

        return this;
    }
}
