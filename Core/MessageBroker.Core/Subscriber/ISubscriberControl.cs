namespace MessageBroker.Core.Subscriber;

public interface ISubscriberControl
{
    bool IsStarted { get; }
    Task Start();
    Task Stop();
}
