using MessageBroker.Core.MessageBroker;

namespace Provider.RabbitMQ;

public static class MessageBrokerBuilderExtensions
{
    public static MessageBrokerBuilder WithProviderRedis(
        this MessageBrokerBuilder mbb,
        Action<RabbitMQMessageBrokerSettings> configure)
    {
        ArgumentNullException.ThrowIfNull(mbb);
        ArgumentNullException.ThrowIfNull(configure);

        var providerSettings = new RabbitMQMessageBrokerSettings();
        configure(providerSettings);

        return mbb.WithProvider(settings =>
            new RabbitMQMessageBroker(settings, providerSettings));
    }
}
