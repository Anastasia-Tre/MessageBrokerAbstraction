using MessageBroker.Core.MessageBroker;

namespace Provider.Mqtt;

public static class MessageBrokerBuilderExtensions
{
    public static MessageBrokerBuilder WithProviderMqtt(
        this MessageBrokerBuilder mbb,
        Action<MqttMessageBrokerSettings> configure)
    {
        if (mbb == null) throw new ArgumentNullException(nameof(mbb));
        if (configure == null)
            throw new ArgumentNullException(nameof(configure));

        var providerSettings = new MqttMessageBrokerSettings();
        configure(providerSettings);

        return mbb.WithProvider(settings =>
            new MqttMessageBroker(settings, providerSettings));
    }
}
