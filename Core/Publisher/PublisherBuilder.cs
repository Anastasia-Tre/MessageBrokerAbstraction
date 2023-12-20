namespace MessageBroker.Core.Publisher;

public class PublisherBuilder<T>
{
    public PublisherSettings Settings { get; }

    public PublisherBuilder(PublisherSettings settings)
        : this(settings, typeof(T))
    {
    }

    public PublisherBuilder(PublisherSettings settings, Type messageType)
    {
        Settings = settings;
        Settings.MessageType = messageType;
    }

    public PublisherBuilder<T> DefaultTopic(string name)
    {
        Settings.DefaultTopic = name;
        return this;
    }

    public PublisherBuilder<T> DefaultTimeout(TimeSpan timeout)
    {
        Settings.ExpiredTimeout = timeout;
        return this;
    }
}