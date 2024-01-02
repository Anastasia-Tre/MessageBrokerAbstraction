using MessageBroker.Core.Message;
using MessageBroker.Core.MessageBroker;
using MessageBroker.Core.Subscriber;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using ISubscriber = StackExchange.Redis.ISubscriber;

namespace Provider.Redis;

public class RedisMessageBroker : MessageBrokerBase<RedisMessageBrokerSettings>
{
    private readonly List<RedisSubscriber> _subscribers = new();

    public RedisMessageBroker(MessageBrokerSettings settings,
        RedisMessageBrokerSettings redisSettings)
        : base(settings, redisSettings)
    {
        IsStarted = false;

        Connection = redisSettings.ConnectionFactory();
        Database = Connection.GetDatabase();

        Start().Wait();
    }

    protected IConnectionMultiplexer Connection { get; }
    protected IDatabase Database { get; }

    public override bool IsStarted { get; set; }

    public override Task Start()
    {
        if (!IsStarted)
        {
            IsStarted = true;
            CreateSubscribers();
        }

        return Task.CompletedTask;
    }

    public override Task Stop()
    {
        if (IsStarted)
        {
            DestroySubscribers();
            IsStarted = false;
        }

        return Task.CompletedTask;
    }

    public override async Task PublishToProvider(Type? messageType,
        object message, string name,
        byte[]? payload)
    {
        var result = await Database.PublishAsync(name, payload)
            .ConfigureAwait(false);
        Logger.LogDebug(
            "Produced message {Message} of type {MessageType} to redis channel {Name} with result {Result}",
            message, messageType, name, result);
    }

    protected void CreateSubscribers()
    {
        var subscriber = Connection.GetSubscriber();

        Logger.LogInformation("Creating subscribers");
        foreach (var subscriberSettings in Settings.Subscribers)
        {
            Logger.LogInformation("Creating subscriber for {Topic}",
                subscriberSettings.Topic);
            var messageProcessor =
                new MessageHandler<byte[]>(subscriberSettings, this, m => m);
            AddSubscriber(subscriberSettings, subscriber, messageProcessor);
        }
    }

    protected void DestroySubscribers()
    {
        Logger.LogInformation("Destroying subscribers");

        _subscribers.ForEach(subscriber => subscriber.Dispose());
        _subscribers.Clear();
    }

    protected void AddSubscriber(SubscriberSettings settings,
        ISubscriber subscriber, IMessageHandler<byte[]> messageProcessor)
    {
        var redisSubscriber =
            new RedisSubscriber(settings, subscriber, messageProcessor);
        _subscribers.Add(redisSubscriber);
    }
}
