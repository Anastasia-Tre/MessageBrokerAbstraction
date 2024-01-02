using MessageBroker.Core.MessageBroker;

namespace Provider.Redis;

public static class MessageBrokerBuilderExtensions
{
    public static MessageBrokerBuilder WithProviderRedis(
        this MessageBrokerBuilder mbb,
        Action<RedisMessageBrokerSettings> configure)
    {
        ArgumentNullException.ThrowIfNull(mbb);
        ArgumentNullException.ThrowIfNull(configure);

        var providerSettings = new RedisMessageBrokerSettings();
        configure(providerSettings);

        return mbb.WithProvider(settings =>
            new RedisMessageBroker(settings, providerSettings));
    }
}
