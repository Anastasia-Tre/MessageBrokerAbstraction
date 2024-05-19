using MessageBroker.Core.Message;
using MessageBroker.Core.MessageBroker;
using MessageBroker.Core.Subscriber;
using Microsoft.Extensions.Logging;
using Serialization.Core;

namespace Provider.Memory;

public class
    MemoryMessageBroker : MessageBrokerBase<MemoryMessageBrokerSettings>
{
    private readonly ILogger _logger;
    private IDictionary<string, IMessageHandler<object>> _subscribersByPath;

    public MemoryMessageBroker(MessageBrokerSettings settings,
        MemoryMessageBrokerSettings providerSettings)
        : base(settings, providerSettings)
    {
        _logger = LoggerFactory.CreateLogger<MessageBrokerBase>();

        OnBuildProvider();
    }

    private async Task ProduceInternal(object message, string path,
        IServiceProvider currentServiceProvider)
    {
        var messageType = message.GetType();
        var publisherSettings = GetPublisherSettings(messageType);
        path ??=
            GetDefaultName(publisherSettings.MessageType, publisherSettings);
        if (!_subscribersByPath.TryGetValue(path, out var messageHandler))
        {
            _logger.LogDebug(
                "No subscribers interested in message type {MessageType} on path {Path}",
                messageType, path);
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
        string name,
        byte[]? payload)
    {
        return Task.CompletedTask;
    }

    protected override void Build()
    {
        base.Build();

        _subscribersByPath = Settings.Subscribers
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
