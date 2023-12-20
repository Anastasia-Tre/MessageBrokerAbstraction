﻿using MessageBroker.Core.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Serialization.Core;

namespace MessageBroker.Core.MessageBroker;

public class MessageBrokerBuilder
{
    public MessageBrokerSettings Settings { get; } = new();
    private Func<MessageBrokerSettings, IMessageBroker> _factory;

    public IList<Action<IServiceCollection>> PostConfigurationActions { get; } = new List<Action<IServiceCollection>>();


    protected MessageBrokerBuilder() {}

    public static MessageBrokerBuilder Create() => new();


    public MessageBrokerBuilder WithSerializer(IMessageSerializer serializer)
    {
        Settings.SerializerType = serializer.GetType();
        return this;
    }

    public MessageBrokerBuilder WithDependencyResolver(IServiceProvider serviceProvider)
    {
        Settings.ServiceProvider = serviceProvider;
        return this;
    }

    public MessageBrokerBuilder WithProvider(Func<MessageBrokerSettings, IMessageBroker> provider)
    {
        _factory = provider;
        return this;
    }

    public MessageBrokerBuilder Do(Action<MessageBrokerBuilder> builder)
    {
        builder(this);
        return this;
    }

    public IMessageBroker Build()
    {
        if (_factory is null)
            throw new ConfigurationMessageBusException("The bus provider was not configured. Check the MessageBus configuration and ensure the has the '.WithProviderXxx()' setting for one of the available transports.");

        return _factory(Settings);
    }
}