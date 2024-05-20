using MessageBroker.Core.Message;
using MessageBroker.Core.MessageBroker;
using MessageBroker.Core.Subscriber;
using Microsoft.Extensions.Logging;
using Serialization.Core;

namespace Provider.Memory;

public class MemoryMessageBroker : 
    MessageBrokerBase<MemoryMessageBrokerSettings>
{
    private readonly ILogger _logger;
    private IDictionary<string, IMessageHandler<object>> _subscribersByTopic;

    public MemoryMessageBroker(MessageBrokerSettings settings,
        MemoryMessageBrokerSettings providerSettings)
        : base(settings, providerSettings)
    {
        _logger = LoggerFactory.CreateLogger<MessageBrokerBase>();

        _ = OnBuildProvider();
    }

    private async Task ProduceInternal(object message, string? topic = null)
    {
        var messageType = message.GetType();
        var publisherSettings = GetPublisherSettings(messageType);
        topic ??=
            GetDefaultTopic(publisherSettings);
        if (!_subscribersByTopic.TryGetValue(topic, out var messageHandler))
        {
            _logger.LogDebug(
                "No subscribers interested in message type {MessageType} on topic {Topic}",
                messageType, topic);
            return;
        }

        var transportMessage = ProviderSettings.EnableMessageSerialization
            ? Serializer.Serialize(publisherSettings.MessageType, message)
            : message;

        await messageHandler.HandleMessage(transportMessage);
    }

    #region Overrides of MessageBusBase

    public override Task Start()
    {
        return Task.CompletedTask;
    }

    public override Task Stop()
    {
        return Task.CompletedTask;
    }

    public override bool IsStarted { get; set; }

    protected override IMessageSerializer GetSerializer()
    {
        if (!ProviderSettings.EnableMessageSerialization)
            return new NullMessageSerializer();
        return base.GetSerializer();
    }

    public override Task PublishToProvider(Type? messageType, object message,
        string name, byte[]? payload)
    {
        return Task.CompletedTask;
    }

    protected override async Task Build()
    {
        await base.Build();

        _subscribersByTopic = Settings.Subscribers
            .GroupBy(x => x.Topic)
            .ToDictionary(
                x => x.Key,
                x => CreateMessageHandler(x.ToList()));
    }

    private IMessageHandler<object> CreateMessageHandler(
        IEnumerable<SubscriberSettings> subscriberSettings)
    {
        return new MessageHandler<object>(subscriberSettings.First(), this,
            ProviderSettings.EnableMessageSerialization
                ? transportMessage =>
                    (byte[])Serializer.Deserialize(transportMessage.GetType(),
                        (byte[])transportMessage)
                : transportMessage => (byte[])transportMessage);
    }

    #endregion
}
