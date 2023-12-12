namespace MessageBroker.Core;

public class MessageBrokerSettings
{
    public IServiceProvider ServiceProvider;

    public Type SerializerType { get; set; }
}