using MessageBroker.Core.MessageBroker;

namespace MessageBroker.Core.Subscriber;

public class SubscriberBuilder<T>
{
    //public Type MessageType { get; }

    public MessageBrokerSettings Settings { get; }

    public SubscriberSettings SubscriberSettings;

    public SubscriberBuilder(MessageBrokerSettings settings, Type messageType, string topic = null)
    {
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        //MessageType = messageType;

        SubscriberSettings = new SubscriberSettings
        {
            MessageType = messageType,
            Topic = topic,
        };
        Settings.Subscribers.Add(SubscriberSettings);
    }

    public SubscriberBuilder(MessageBrokerSettings settings)
        : this(settings, typeof(T))
    {
    }

    public SubscriberBuilder<T> Topic(string topic)
    {
        SubscriberSettings.Topic = topic;
        return this;
    }


    public SubscriberBuilder<T> WithSubscriber<TSubscriber>()
        where TSubscriber : class, ISubscriber<T>
    {
        SubscriberSettings.SubscriberType = typeof(TSubscriber);
        SubscriberSettings.SubscriberMethod = (consumer, message) => ((ISubscriber<T>)consumer).OnHandle((T)message);

        SubscriberSettings.Invokers.Add(SubscriberSettings);

        return this;
    }

    public SubscriberBuilder<T> WithSubscriber<TSubscriber, TMessage>()
        where TSubscriber : class, ISubscriber<TMessage>
        where TMessage : T
    {
        var invoker = new SubscriberSettings()
        {
            SubscriberMethod = (consumer, message) => ((ISubscriber<TMessage>)consumer).OnHandle((TMessage)message)
        };
        SubscriberSettings.Invokers.Add(invoker);

        return this;
    }

    public SubscriberBuilder<T> WithSubscriber<TSubscriber>(Func<TSubscriber, T, Task> consumerMethod)
        where TSubscriber : class
    {
        if (consumerMethod == null) throw new ArgumentNullException(nameof(consumerMethod));

        SubscriberSettings.SubscriberType = typeof(TSubscriber);
        SubscriberSettings.SubscriberMethod = (consumer, message) => consumerMethod((TSubscriber)consumer, (T)message);

        SubscriberSettings.Invokers.Add(SubscriberSettings);

        return this;
    }
}