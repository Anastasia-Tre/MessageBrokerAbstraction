namespace MessageBroker.Core.MessageBroker;

public class MessageBrokerSettings
{
    public IServiceProvider ServiceProvider;

    public Type SerializerType { get; set; }
}