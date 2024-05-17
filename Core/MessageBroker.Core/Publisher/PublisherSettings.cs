using MessageBroker.Core.MessageBroker;

namespace MessageBroker.Core.Publisher;

public class PublisherSettings : BaseSettings
{
    public Type? MessageType { get; set; }

    public string? DefaultTopic { get; set; }

    public TimeSpan? ExpiredTimeout { get; set; }
}
