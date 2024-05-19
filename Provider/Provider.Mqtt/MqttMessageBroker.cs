using Microsoft.Extensions.Logging;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessageBroker.Core.Message;
using MessageBroker.Core.MessageBroker;

namespace Provider.Mqtt
{
    public class MqttMessageBroker : MessageBrokerBase<MqttMessageBrokerSettings>
    {
        private readonly ILogger _logger;
        private IManagedMqttClient _mqttClient;

        public MqttMessageBroker(MessageBrokerSettings settings, MqttMessageBrokerSettings providerSettings) : base(settings, providerSettings)
        {
            _logger = LoggerFactory.CreateLogger<MqttMessageBroker>();

            OnBuildProvider();
        }
        
        public bool IsConnected => _mqttClient?.IsConnected ?? false;

        public override async Task Start()
        {
            var clientOptions = ProviderSettings.ClientBuilder
                .Build();

            var managedClientOptions = ProviderSettings.ManagedClientBuilder
                .WithClientOptions(clientOptions)
                .Build();

            await _mqttClient.StartAsync(managedClientOptions).ConfigureAwait(false);
        }

        public override async Task Stop()
        {
            if (_mqttClient != null)
            {
                await _mqttClient.StopAsync().ConfigureAwait(false);
            }
        }

        protected override async Task Build()
        {
            _mqttClient = ProviderSettings.MqttFactory.CreateManagedMqttClient();
            _mqttClient.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;

            byte[] MessageProvider(MqttApplicationMessage transportMessage) => (byte[])Serializer.Deserialize(transportMessage.GetType(), transportMessage.PayloadSegment.Array);

            void AddTopicSubscriber(string topic, IMessageHandler<MqttApplicationMessage> messageHandler)
            {
                _logger.LogInformation("Creating consumer for {Path}", topic);
                var consumer = new MqttSubscriber(LoggerFactory.CreateLogger<MqttSubscriber>(), topic, messageHandler);
                AddSubscriber(consumer);
            }

            _logger.LogInformation("Creating consumers");
            foreach (var (path, consumerSettings) in Settings.Subscribers.GroupBy(x => x.Topic).ToDictionary(x => x.Key, x => x.ToList()))
            {
                var handler = new MessageHandler<MqttApplicationMessage>(consumerSettings.First(), this, MessageProvider);
                AddTopicSubscriber(path, handler);
            }

            var topics = Subscribers.Cast<MqttSubscriber>().Select(x => new MqttTopicFilterBuilder().WithTopic(x.Topic).Build()).ToList();
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

        private Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
        {
            var consumer = Subscribers.Cast<MqttSubscriber>().FirstOrDefault(x => x.Topic == arg.ApplicationMessage.Topic);
            if (consumer != null)
            {
                var headers = new Dictionary<string, object>();
                if (arg.ApplicationMessage.UserProperties != null)
                {
                    foreach (var prop in arg.ApplicationMessage.UserProperties)
                    {
                        headers[prop.Name] = prop.Value;
                    }
                }
                return consumer.MessageHandler.HandleMessage(arg.ApplicationMessage);
            }
            return Task.CompletedTask;
        }

        public override async Task PublishToProvider(Type? messageType, object message, string path, byte[] messagePayload)
        {
            var m = new MqttApplicationMessage
            {
                PayloadSegment = messagePayload,
                Topic = path
            };

            try
            {
                var messageModifier = Settings.GetMessageModifier();
                messageModifier?.Invoke(message, m);

                if (messageType != null)
                {
                    var publisherSettings = GetPublisherSettings(messageType);
                    messageModifier = publisherSettings.GetMessageModifier();
                    messageModifier?.Invoke(message, m);
                }
            } catch (Exception e)
            {
                _logger.LogWarning(e, "The configured message modifier failed for message type {MessageType} and message {Message}", messageType, message);
            }

            await _mqttClient.EnqueueAsync(m);
        }

        public override bool IsStarted { get; set; }
    }
}
