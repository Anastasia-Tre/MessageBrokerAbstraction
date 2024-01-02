using MessageBroker.Core.Message;
using MessageBroker.Core.Subscriber;
using StackExchange.Redis;
using ISubscriber = StackExchange.Redis.ISubscriber;

namespace Provider.Redis;

internal class RedisSubscriber : IDisposable
{
    private readonly ChannelMessageQueue _channelMessageQueue;

    public RedisSubscriber(SubscriberSettings settings, ISubscriber subscriber,
        IMessageHandler<byte[]> messageProcessor)
    {
        _channelMessageQueue = subscriber.Subscribe(settings.Topic!);
        _channelMessageQueue.OnMessage(m =>
            messageProcessor.HandleMessage(m.Message!));
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing) _channelMessageQueue.Unsubscribe();
    }
}
