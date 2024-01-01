using MessageBroker.Core.Publisher;
using MessageBroker.Core.Subscriber;

namespace MessageBroker.Core.MessageBroker;

public class MessageBrokerSettings
{
    public IServiceProvider ServiceProvider;

    public Type SerializerType { get; set; }

    public IList<PublisherSettings> Publishers { get; }
    public IList<SubscriberSettings> Subscribers { get; }
}