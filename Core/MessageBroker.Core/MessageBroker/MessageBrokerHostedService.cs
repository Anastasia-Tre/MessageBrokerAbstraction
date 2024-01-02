using MessageBroker.Core.Subscriber;
using Microsoft.Extensions.Hosting;

namespace MessageBroker.Core.MessageBroker;

public class MessageBrokerHostedService : IHostedService
{
    private readonly ISubscriberControl _broker;
    private readonly MessageBrokerSettings _messageBrokerSettings;

    public MessageBrokerHostedService(ISubscriberControl broker,
        MessageBrokerSettings messageBrokerSettings)
    {
        _broker = broker;
        _messageBrokerSettings = messageBrokerSettings;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _broker.Start().ConfigureAwait(false);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return _broker.Stop();
    }
}
