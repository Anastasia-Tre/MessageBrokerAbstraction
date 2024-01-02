using MessageBroker.Core.Exceptions;
using MessageBroker.Core.Publisher;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Serialization.Core;

namespace MessageBroker.Core.MessageBroker;

public abstract class MessageBrokerBase : IMessageBroker, IAsyncDisposable
{
    protected readonly ILogger Logger;
    public ILoggerFactory LoggerFactory { get; }

    private CancellationTokenSource? _cancellationTokenSource = new();
    public CancellationToken CancellationToken =>
        _cancellationTokenSource!.Token;

    private IMessageSerializer? _serializer;
    public virtual IMessageSerializer Serializer
    {
        get {
            _serializer ??= GetSerializer();
            return _serializer;
        }
    }

    protected MessageBrokerBase(MessageBrokerSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        AssertSettings(settings);
        Settings = settings;

        PublisherSettingsByMessageType =
            new Dictionary<Type?, PublisherSettings>();
        foreach (var publisherSettings in settings.Publishers)
        {
            if (PublisherSettingsByMessageType.ContainsKey(publisherSettings.MessageType))
                throw new InvalidConfigurationMessageBrokerException(
                    $"The produced message type '{publisherSettings.MessageType}' was declared more than once (check the {nameof(MessageBrokerBuilder)} configuration)");
            PublisherSettingsByMessageType.Add(publisherSettings.MessageType,
                publisherSettings);
        }

        LoggerFactory = settings.ServiceProvider.GetService<ILoggerFactory>() ??
                        NullLoggerFactory.Instance;
        Logger = LoggerFactory.CreateLogger<MessageBrokerBase>();
    }

    public virtual MessageBrokerSettings Settings { get; }

    protected IDictionary<Type?, PublisherSettings> PublisherSettingsByMessageType { get; }

    public virtual DateTimeOffset CurrentTime => DateTimeOffset.UtcNow;

    public abstract Task Start();
    public abstract bool IsStarted { get; set; }
    public abstract Task Stop();

    public Task Publish<TMessage>(TMessage message, string name = null)
    {
        return Publish(message!.GetType(), message, name);
    }

    protected virtual IMessageSerializer GetSerializer()
    {
        return (IMessageSerializer)Settings.ServiceProvider.GetService(
                   Settings.SerializerType)!
               ?? throw new ConfigurationMessageBrokerException(
                   $"The broker could not resolve the required message serializer type {Settings.SerializerType.Name} from {nameof(Settings.ServiceProvider)}");
    }

    private void AssertSettings(MessageBrokerSettings settings)
    {
        if (settings.SerializerType == null)
            throw new InvalidConfigurationMessageBrokerException(
                $"{nameof(MessageBrokerSettings.SerializerType)} was not set on {nameof(MessageBrokerSettings)} object");

        if (settings.ServiceProvider == null)
            throw new InvalidConfigurationMessageBrokerException(
                $"{nameof(MessageBrokerSettings.ServiceProvider)} was not set on {nameof(MessageBrokerSettings)} object");
    }

    protected void AssertActive()
    {
        if (IsDisposed || IsDisposing)
            throw new MessageBrokerException("The message broker is disposed at this time");
    }

    public abstract Task PublishToProvider(Type? messageType, object message,
        string name, byte[]? payload);

    public virtual Task Publish(Type? messageType, object message,
        string? name = null)
    {
        AssertActive();

        name ??= GetDefaultName(messageType);

        var payload = SerializeMessage(messageType, message);

        Logger.LogDebug(
            "Producing message {Message} of type {MessageType} to name {Name} with payload size {Size}",
            message, messageType, name, payload?.Length ?? 0);
        return PublishToProvider(messageType, message, name, payload);
    }

    protected PublisherSettings GetPublisherSettings(Type? messageType)
    {
        if (!PublisherSettingsByMessageType.TryGetValue(messageType,
                out var publisherSettings))
            throw new PublishMessageBrokerException(
                $"Message of type {messageType} was not registered as a supported publish message. Please check your MessageBroker configuration and include this type.");

        return publisherSettings;
    }

    protected virtual string GetDefaultName(Type? messageType)
    {
        var publisherSettings = GetPublisherSettings(messageType);
        return GetDefaultName(messageType, publisherSettings);
    }

    protected virtual string GetDefaultName(Type? messageType,
        PublisherSettings publisherSettings)
    {
        var name = publisherSettings.DefaultTopic ?? throw new PublishMessageBrokerException(
                $"An attempt to produce message of type {messageType} without specifying name, but there was no default name configured. Double check your configuration.");
        Logger.LogDebug("Applying default name {Name} for message type {MessageType}", name,
            messageType);
        return name;
    }

    public virtual byte[] SerializeMessage(Type? messageType, object message)
    {
        return Serializer.Serialize(messageType, message);
    }

    public virtual object DeserializeMessage(Type messageType, byte[] payload)
    {
        return Serializer.Deserialize(messageType, payload);
    }

    #region dispose

    protected bool IsDisposing { get; private set; }
    protected bool IsDisposed { get; private set; }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected void Dispose(bool disposing)
    {
        if (disposing)
            DisposeAsyncInternal().ConfigureAwait(false).GetAwaiter()
                .GetResult();
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncInternal().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    private async ValueTask DisposeAsyncInternal()
    {
        if (!IsDisposed && !IsDisposing)
        {
            IsDisposing = true;
            try
            {
                await DisposeAsyncCore().ConfigureAwait(false);
            }
            finally
            {
                IsDisposed = true;
                IsDisposing = false;
            }
        }
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_cancellationTokenSource != null)
        {
            await _cancellationTokenSource.CancelAsync().ConfigureAwait(false);
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }
    }

    #endregion
}
