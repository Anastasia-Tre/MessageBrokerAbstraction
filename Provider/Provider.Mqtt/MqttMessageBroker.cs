using MessageBroker.Core.Message;
using MessageBroker.Core.MessageBroker;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;

namespace Provider.Mqtt;

public class MqttMessageBroker : MessageBrokerBase<MqttMessageBrokerSettings>
{
    private readonly ILogger _logger;
    private IManagedMqttClient? _mqttClient;

    public MqttMessageBroker(MessageBrokerSettings settings,
        MqttMessageBrokerSettings providerSettings) : base(settings,
        providerSettings)
    {
        _logger = LoggerFactory.CreateLogger<MqttMessageBroker>();

        _ = OnBuildProvider();
    }

    public bool IsConnected => _mqttClient?.IsConnected ?? false;

    public override bool IsStarted { get; set; }

    public override async Task Start()
    {
        var clientOptions = ProviderSettings.ClientBuilder
            .Build();

        var managedClientOptions = ProviderSettings.ManagedClientBuilder
            .WithClientOptions(clientOptions)
            .Build();

        if (_mqttClient != null)
            await _mqttClient.StartAsync(managedClientOptions)
                             .ConfigureAwait(false);
    }

    public override async Task Stop()
    {
        if (_mqttClient != null)
            await _mqttClient.StopAsync().ConfigureAwait(false);
    }

    protected override async Task Build()
    {
        _mqttClient = ProviderSettings.MqttFactory.CreateManagedMqttClient();
        _mqttClient.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;

        byte[] MessageProvider(MqttApplicationMessage transportMessage)
        {
            return (byte[])Serializer.Deserialize(transportMessage.GetType(),
                transportMessage.PayloadSegment.Array!);
        }

        void AddTopicSubscriber(string topic,
            IMessageHandler<MqttApplicationMessage> messageHandler)
        {
            _logger.LogInformation("Creating consumer for {Topic}", topic);
            var consumer = new MqttSubscriber(
                LoggerFactory.CreateLogger<MqttSubscriber>(), topic,
                messageHandler);
            AddSubscriber(consumer);
        }

        _logger.LogInformation("Creating consumers");
        foreach (var (topic, consumerSettings) in Settings.Subscribers
                     .GroupBy(x => x.Topic)
                     .ToDictionary(x => x.Key, x => x.ToList()))
        {
            var handler =
                new MessageHandler<MqttApplicationMessage>(
                    consumerSettings.First(), this, MessageProvider);
            AddTopicSubscriber(topic, handler);
        }

        var topics = Subscribers.Cast<MqttSubscriber>().Select(x =>
            new MqttTopicFilterBuilder().WithTopic(x.Topic).Build()).ToList();
        await _mqttClient.SubscribeAsync(topics).ConfigureAwait(false);
    }

    protected override async ValueTask DisposeInternalAsync()
    {
        await base.DisposeInternalAsync();

        if (_mqttClient != null)
        {
            _mqttClient.Dispose();
            _mqttClient = null;
        }
    }

    private Task OnMessageReceivedAsync(
        MqttApplicationMessageReceivedEventArgs arg)
    {
        var consumer = Subscribers.Cast<MqttSubscriber>()
            .FirstOrDefault(x => x.Topic == arg.ApplicationMessage.Topic);
        if (consumer != null)
        {
            return consumer.MessageHandler.HandleMessage(
                arg.ApplicationMessage);
        }

        return Task.CompletedTask;
    }

    public override async Task PublishToProvider(Type? messageType,
        object message, string topic, byte[] messagePayload)
    {
        var mqttMessage = new MqttApplicationMessage
        {
            PayloadSegment = messagePayload,
            Topic = topic
        };

        try
        {
            var messageModifier = Settings.GetMessageModifier();
            messageModifier?.Invoke(message, mqttMessage);

            if (messageType != null)
            {
                var publisherSettings = GetPublisherSettings(messageType);
                messageModifier = publisherSettings.GetMessageModifier();
                messageModifier?.Invoke(message, mqttMessage);
            }
        }
        catch (Exception e)
        {
            _logger.LogWarning(e,
                "The configured message modifier failed for message type {MessageType} and message {Message}",
                messageType, message);
        }

        await _mqttClient.EnqueueAsync(mqttMessage);
    }
}
