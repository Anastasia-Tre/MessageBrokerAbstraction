namespace MessageBroker.Core.Subscriber;

public interface ISubscriberControl
{
    Task Start();

    bool IsStarted { get; }

    Task Stop();
}