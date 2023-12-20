namespace MessageBroker.Core.Subscriber;

public class SubscriberSettings : ISubscriberEvent
{
    public string Topic { get; set; }

    public Type ConsumerType { get; set; }
    public Type MessageType { get; set; }

    public Action<SubscriberSettings, object> OnMessageExpired { get; set; }
    public Action<SubscriberSettings, object, Exception> OnMessageFault { get; set; }

    protected SubscriberSettings() { }
}