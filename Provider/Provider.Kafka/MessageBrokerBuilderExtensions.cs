using System;
using MessageBroker.Core.MessageBroker;

namespace Provider.Kafka;

public static class MessageBrokerBuilderExtensions
{
    public static MessageBrokerBuilder WithProviderRedis(
        this MessageBrokerBuilder mbb,
        Action<KafkaMessageBrokerSettings> configure)
    {
        ArgumentNullException.ThrowIfNull(mbb);
        ArgumentNullException.ThrowIfNull(configure);

        var providerSettings = new KafkaMessageBrokerSettings();
        configure(providerSettings);

        return mbb.WithProvider(settings =>
            new KafkaMessageBroker(settings, providerSettings));
    }
}
