using MessageBroker.Core.MessageBroker;
using MQTTnet;

namespace Provider.Mqtt;

public static class BaseSettingsExtensions
{
    internal static BaseSettings SetMessageModifier(
        this BaseSettings producerSettings,
        Action<object, MqttApplicationMessage> messageModifierAction)
    {
        producerSettings.Properties[nameof(SetMessageModifier)] =
            messageModifierAction;
        return producerSettings;
    }

    internal static Action<object, MqttApplicationMessage> GetMessageModifier(
        this BaseSettings producerSettings)
    {
        return producerSettings
            .GetOrDefault<Action<object, MqttApplicationMessage>>(
                nameof(SetMessageModifier));
    }
}
