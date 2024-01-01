using Microsoft.Extensions.Logging;

namespace MessageBroker.Core.Subscriber;

public abstract class SubscriberBase : IAsyncDisposable, ISubscriberControl, ISubscriberEvent
{
    private CancellationTokenSource _cancellationTokenSource;
    private bool _starting;
    private bool _stopping;

    protected ILogger Logger { get; }

    public bool IsStarted { get; private set; }

    protected CancellationToken CancellationToken => _cancellationTokenSource.Token;

    protected SubscriberBase(ILogger logger)
    {
        Logger = logger;
    }

    public async Task Start()
    {
        if (IsStarted || _starting)
            return;

        _starting = true;
        try
        {
            if (_cancellationTokenSource == null || _cancellationTokenSource.IsCancellationRequested)
            {
                await _cancellationTokenSource?.CancelAsync();
                _cancellationTokenSource = new CancellationTokenSource();
            }

            await OnStart().ConfigureAwait(false);

            IsStarted = true;
        } finally
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
            await _cancellationTokenSource.CancelAsync();

            await OnStop().ConfigureAwait(false);

            IsStarted = false;
        } finally
        {
            _stopping = false;
        }
    }

    protected abstract Task OnStart();
    protected abstract Task OnStop();

    public abstract Action<SubscriberSettings, object> OnMessageExpired { get; set; }
    public abstract Action<SubscriberSettings, object, Exception> OnMessageFault { get; set; }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        await Stop().ConfigureAwait(false);

        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
    }
}
