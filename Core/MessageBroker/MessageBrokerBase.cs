using MessageBroker.Core.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Serialization.Core;

namespace MessageBroker.Core.MessageBroker;

public abstract class MessageBrokerBase : IMessageBroker, IAsyncDisposable
{
    private readonly ILogger _logger;
    public ILoggerFactory LoggerFactory { get; }

    public virtual MessageBrokerSettings Settings { get; }

    private CancellationTokenSource _cancellationTokenSource = new();
    public CancellationToken CancellationToken => _cancellationTokenSource.Token;

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
        ?? throw new ConfigurationMessageBusException($"The broker could not resolve the required message serializer type {Settings.SerializerType.Name} from {nameof(Settings.ServiceProvider)}");


    protected MessageBrokerBase(MessageBrokerSettings settings)
    {
        if (settings is null) throw new ArgumentNullException(nameof(settings));
        if (settings.ServiceProvider is null) throw new ConfigurationMessageBusException($"The broker has no {nameof(settings.ServiceProvider)} configured");

        Settings = settings;

        LoggerFactory = settings.ServiceProvider.GetService<ILoggerFactory>() ?? NullLoggerFactory.Instance;
        _logger = LoggerFactory.CreateLogger<MessageBrokerBase>();
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

    public Task Publish<TMessage>(TMessage message)
    {
        throw new NotImplementedException();
    }
}