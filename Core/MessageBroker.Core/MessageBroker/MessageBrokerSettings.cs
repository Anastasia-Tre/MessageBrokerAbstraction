using MessageBroker.Core.Publisher;
using MessageBroker.Core.Subscriber;
using Serialization.Core;

namespace MessageBroker.Core.MessageBroker;

public class MessageBrokerSettings
{
    public IServiceProvider? ServiceProvider;

    public MessageBrokerSettings()
    {
        Publishers = new List<PublisherSettings>();
        Subscribers = new List<SubscriberSettings>();
        SerializerType = typeof(IMessageSerializer);
    }

    public Type SerializerType { get; set; }

    public IList<PublisherSettings> Publishers { get; }
    public IList<SubscriberSettings> Subscribers { get; }
}
