namespace MessageBroker.Core.Subscriber;

public interface ISubscriber
{
}

public interface ISubscriber<in TMessage> : ISubscriber
{
    Task OnHandle(TMessage message);
}
