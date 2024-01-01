using MessageBroker.Core.MessageBroker;
using StackExchange.Redis;
using System.Globalization;
using MessageBroker.Core.Message;
using MessageBroker.Core.Subscriber;
using Microsoft.Extensions.Logging;
using ISubscriber = StackExchange.Redis.ISubscriber;

namespace Provider.Redis;

public class RedisMessageBroker : MessageBrokerBase<RedisMessageBrokerSettings>
{
    protected IConnectionMultiplexer Connection { get; private set; }
    protected IDatabase Database { get; private set; }

    private readonly List<RedisSubscriber> _subscribers = new();

    public RedisMessageBroker(MessageBrokerSettings settings, RedisMessageBrokerSettings redisSettings) 
        : base(settings, redisSettings)
    {
        IsStarted = false;

        Connection = redisSettings.ConnectionFactory();
        Database = Connection.GetDatabase();

        Start().Wait();
    }

    public override Task Start()
    {
        if (!IsStarted)
        {
            IsStarted = true;
            CreateSubscribers();
        }
        return Task.CompletedTask;
    }

    public override bool IsStarted { get; set; }
    public override Task Stop()
    {
        if (IsStarted)
        {
            DestroySubscribers();
            IsStarted = false;
        }
        return Task.CompletedTask;
    }

    public override async Task PublishToProvider(Type messageType, object message, string name,
        byte[] payload)
    {
        var result = await Database.PublishAsync(name, payload).ConfigureAwait(false);
        _logger.LogDebug("Produced message {0} of type {1} to redis channel {2} with result {3}", message, messageType, name, result);
    }

    protected void CreateSubscribers()
    {
        var subscriber = Connection.GetSubscriber();

        _logger.LogInformation("Creating subscribers");
        foreach (var subscriberSettings in Settings.Subscribers)
        {
            _logger.LogInformation("Creating subscriber for {0}", subscriberSettings.Topic);
            var messageProcessor = new MessageHandler<byte[]>(subscriberSettings, this, m => m);
            AddSubscriber(subscriberSettings, subscriber, messageProcessor);
        }
    }

    protected void DestroySubscribers()
    {
        _logger.LogInformation("Destroying subscribers");

        _subscribers.ForEach(subscriber => subscriber.Dispose());
        _subscribers.Clear();
    }

    protected void AddSubscriber(SubscriberSettings settings, ISubscriber subscriber, IMessageHandler<byte[]> messageProcessor)
    {
        var redisSubscriber = new RedisSubscriber(settings, subscriber, messageProcessor);
        _subscribers.Add(redisSubscriber);
    }
}