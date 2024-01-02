using Microsoft.Extensions.Logging;

namespace MessageBroker.Core.Subscriber;

public abstract class SubscriberBase : IAsyncDisposable, ISubscriberControl,
    ISubscriberEvent
{
    private CancellationTokenSource? _cancellationTokenSource;
    protected CancellationToken CancellationToken =>
        _cancellationTokenSource!.Token;

    private bool _starting;
    private bool _stopping;

    protected SubscriberBase(ILogger logger)
    {
        Logger = logger;
    }

    protected ILogger Logger { get; }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    public bool IsStarted { get; private set; }

    public async Task Start()
    {
        if (IsStarted || _starting)
            return;

        _starting = true;
        try
        {
            if (_cancellationTokenSource == null ||
                _cancellationTokenSource.IsCancellationRequested)
            {
                await _cancellationTokenSource?.CancelAsync()!;
                _cancellationTokenSource = new CancellationTokenSource();
            }

            await OnStart().ConfigureAwait(false);

            IsStarted = true;
        }
        finally
        {
            _starting = false;
        }
    }

    public async Task Stop()
    {
        if (!IsStarted || _stopping)
            return;

        _stopping = true;
        try
        {
            await _cancellationTokenSource!.CancelAsync().ConfigureAwait(false);

            await OnStop().ConfigureAwait(false);

            IsStarted = false;
        }
        finally
        {
            _stopping = false;
        }
    }

    public abstract Action<SubscriberSettings, object> OnMessageExpired
    {
        get;
        set;
    }

    public abstract Action<SubscriberSettings, object, Exception> OnMessageFault
    {
        get;
        set;
    }

    protected abstract Task OnStart();
    protected abstract Task OnStop();

    protected virtual async ValueTask DisposeAsyncCore()
    {
        await Stop().ConfigureAwait(false);

        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
    }
}
