﻿using MessageBroker.Core.MessageBroker;
using MQTTnet;

namespace Provider.Mqtt;

public static class BaseSettingsExtensions
{
    internal static BaseSettings SetMessageModifier(
        this BaseSettings settings,
        Action<object, MqttApplicationMessage> messageModifierAction)
    {
        settings.Properties[nameof(SetMessageModifier)] =
            messageModifierAction;
        return settings;
    }

    internal static Action<object, MqttApplicationMessage> GetMessageModifier(
        this BaseSettings settings)
    {
        return settings
            .GetOrDefault<Action<object, MqttApplicationMessage>>(
                nameof(SetMessageModifier));
    }
}
