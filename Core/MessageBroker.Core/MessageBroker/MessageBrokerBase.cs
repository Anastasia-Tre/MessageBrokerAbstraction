using MessageBroker.Core.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Serialization.Core;
using System.Globalization;
using System.Xml.Linq;
using MessageBroker.Core.Publisher;

namespace MessageBroker.Core.MessageBroker;

public abstract class MessageBrokerBase : IMessageBroker, IAsyncDisposable
{
    protected readonly ILogger _logger;
    public ILoggerFactory LoggerFactory { get; }

    public virtual MessageBrokerSettings Settings { get; }
    protected IDictionary<Type, PublisherSettings> PublisherSettingsByMessageType { get; }

    private CancellationTokenSource _cancellationTokenSource = new();
    public CancellationToken CancellationToken => _cancellationTokenSource.Token;

    public virtual DateTimeOffset CurrentTime => DateTimeOffset.UtcNow;

    public abstract Task Start();
    public abstract bool IsStarted { get; set; }
    public abstract Task Stop();

    private IMessageSerializer _serializer;
    public virtual IMessageSerializer Serializer
    {
        get {
            _serializer ??= GetSerializer();
            return _serializer;
        }
    }

    protected virtual IMessageSerializer GetSerializer() =>
        (IMessageSerializer)Settings.ServiceProvider.GetService(
            Settings.SerializerType)
        ?? throw new ConfigurationMessageBrokerException($"The broker could not resolve the required message serializer type {Settings.SerializerType.Name} from {nameof(Settings.ServiceProvider)}");


    protected MessageBrokerBase(MessageBrokerSettings settings)
    {
        if (settings is null) throw new ArgumentNullException(nameof(settings));
        AssertSettings(settings);
        Settings = settings;

        PublisherSettingsByMessageType = new Dictionary<Type, PublisherSettings>();
        foreach (var publisherSettings in settings.Publishers)
        {
            if (PublisherSettingsByMessageType.ContainsKey(publisherSettings.MessageType))
            {
                throw new InvalidConfigurationMessageBrokerException($"The produced message type '{publisherSettings.MessageType}' was declared more than once (check the {nameof(MessageBrokerBuilder)} configuration)");
            }
            PublisherSettingsByMessageType.Add(publisherSettings.MessageType, publisherSettings);
        }

        LoggerFactory = settings.ServiceProvider.GetService<ILoggerFactory>() ?? NullLoggerFactory.Instance;
        _logger = LoggerFactory.CreateLogger<MessageBrokerBase>();
    }

    private void AssertSettings(MessageBrokerSettings settings)
    {
        if (settings.SerializerType == null)
            throw new InvalidConfigurationMessageBrokerException($"{nameof(MessageBrokerSettings.SerializerType)} was not set on {nameof(MessageBrokerSettings)} object");

        if (settings.ServiceProvider == null)
            throw new InvalidConfigurationMessageBrokerException($"{nameof(MessageBrokerSettings.ServiceProvider)} was not set on {nameof(MessageBrokerSettings)} object");
    }

    protected void AssertActive()
    {
        if (IsDisposed || IsDisposing)
        {
            throw new MessageBrokerException("The message broker is disposed at this time");
        }
    }

    #region dispose
    protected bool IsDisposing { get; private set; }
    protected bool IsDisposed { get; private set; }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected void Dispose(bool disposing)
    {
        if (disposing)
        {
            DisposeAsyncInternal().ConfigureAwait(false).GetAwaiter().GetResult();
        }
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
            } finally
            {
                IsDisposed = true;
                IsDisposing = false;
            }
        }
    }

    protected async virtual ValueTask DisposeAsyncCore()
    {
        //await Stop().ConfigureAwait(false);

        if (_cancellationTokenSource != null)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }
    }
    #endregion

    public Task Publish<TMessage>(TMessage message, string name = null)
    {
        return Publish(message.GetType(), message, name);
    }

    public abstract Task PublishToProvider(Type messageType, object message, string name, byte[] payload);

    public virtual Task Publish(Type messageType, object message, string name = null)
    {
        AssertActive();

        if (name == null)
        {
            name = GetDefaultName(messageType);
        }

        var payload = SerializeMessage(messageType, message);

        _logger.LogDebug("Producing message {0} of type {1} to name {2} with payload size {3}", message, messageType, name, payload?.Length ?? 0);
        return PublishToProvider(messageType, message, name, payload);
    }

    protected PublisherSettings GetPublisherSettings(Type messageType)
    {
        if (!PublisherSettingsByMessageType.TryGetValue(messageType, out var publisherSettings))
        {
            throw new PublishMessageBrokerException($"Message of type {messageType} was not registered as a supported publish message. Please check your MessageBroker configuration and include this type.");
        }

        return publisherSettings;
    }

    protected virtual string GetDefaultName(Type messageType)
    {
        // when topic was not provided, lookup default topic from configuration
        var publisherSettings = GetPublisherSettings(messageType);
        return GetDefaultName(messageType, publisherSettings);
    }

    protected virtual string GetDefaultName(Type messageType, PublisherSettings publisherSettings)
    {
        var name = publisherSettings.DefaultTopic;
        if (name == null)
        {
            throw new PublishMessageBrokerException($"An attempt to produce message of type {messageType} without specifying name, but there was no default name configured. Double check your configuration.");
        }
        _logger.LogDebug("Applying default name {0} for message type {1}", name, messageType);
        return name;
    }

    public virtual byte[] SerializeMessage(Type messageType, object message)
    {
        return Serializer.Serialize(messageType, message);
    }

    public virtual object DeserializeMessage(Type messageType, byte[] payload)
    {
        return Serializer.Deserialize(messageType, payload);
    }
}