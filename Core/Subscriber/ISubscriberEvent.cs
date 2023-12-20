namespace MessageBroker.Core.Subscriber;

public interface ISubscriberEvent
{
    Action<SubscriberSettings, object> OnMessageExpired { get; set; }

    Action<SubscriberSettings, object, Exception> OnMessageFault { get; set; }
}