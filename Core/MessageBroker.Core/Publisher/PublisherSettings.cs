namespace MessageBroker.Core.Publisher;

public class PublisherSettings
{
    public Type? MessageType { get; set; }

    public string? DefaultTopic { get; set; }

    public TimeSpan? ExpiredTimeout { get; set; }

    public IDictionary<string, object> Properties { get; protected set; }

    protected PublisherSettings()
    {
        Properties = new Dictionary<string, object>();
    }

    public T GetOrDefault<T>(string key, T defaultValue)
    {
        if (Properties.TryGetValue(key, out var value))
            return (T)value;
        
        return defaultValue;
    }
}
