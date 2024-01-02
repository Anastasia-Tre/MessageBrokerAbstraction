using MessageBroker.Core.Exceptions;
using MessageBroker.Core.Publisher;
using MessageBroker.Core.Subscriber;
using Microsoft.Extensions.DependencyInjection;
using Serialization.Core;

namespace MessageBroker.Core.MessageBroker;

public class MessageBrokerBuilder
{
    private Func<MessageBrokerSettings, IMessageBroker>? _factory;


    protected MessageBrokerBuilder()
    {
    }

    public MessageBrokerSettings Settings { get; } = new();

    public IList<Action<IServiceCollection>> PostConfigurationActions { get; } =
        new List<Action<IServiceCollection>>();

    public static MessageBrokerBuilder Create()
    {
        return new MessageBrokerBuilder();
    }

    public MessageBrokerBuilder SetPublisher<T>(
        Action<PublisherBuilder<T>> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var item = new PublisherSettings();
        builder(new PublisherBuilder<T>(item));
        Settings.Publishers.Add(item);
        return this;
    }

    public MessageBrokerBuilder SetPublisher(Type? messageType,
        Action<PublisherBuilder<object>> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var item = new PublisherSettings();
        builder(new PublisherBuilder<object>(item, messageType));
        Settings.Publishers.Add(item);
        return this;
    }

    public MessageBrokerBuilder SetSubscriber<TMessage>(
        Action<SubscriberBuilder<TMessage>> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var b = new SubscriberBuilder<TMessage>(Settings);
        builder(b);

        if (b.SubscriberSettings.SubscriberType is null)
            b.WithSubscriber<ISubscriber<TMessage>>();
        return this;
    }

    public MessageBrokerBuilder SetSubscriber(Type messageType,
        Action<SubscriberBuilder<object>> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder(new SubscriberBuilder<object>(Settings, messageType));
        return this;
    }

    public MessageBrokerBuilder WithSerializer(IMessageSerializer serializer)
    {
        Settings.SerializerType = serializer.GetType();
        return this;
    }

    public MessageBrokerBuilder WithDependencyResolver(
        IServiceProvider? serviceProvider)
    {
        Settings.ServiceProvider = serviceProvider;
        return this;
    }

    public MessageBrokerBuilder WithProvider(
        Func<MessageBrokerSettings, IMessageBroker>? provider)
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
            throw new ConfigurationMessageBrokerException(
                "The broker provider was not configured. Check the MessageBroker configuration and ensure the has the '.WithProviderXxx()' setting for one of the available providers.");

        return _factory(Settings);
    }
}
