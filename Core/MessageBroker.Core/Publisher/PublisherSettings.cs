namespace MessageBroker.Core.Publisher;

public class PublisherSettings
{
    public Type? MessageType { get; set; }

    public string? DefaultTopic { get; set; }

    public TimeSpan? ExpiredTimeout { get; set; }
}
