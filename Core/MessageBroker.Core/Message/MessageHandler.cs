using System.Collections;
using System.Threading.Tasks.Dataflow;
using MessageBroker.Core.MessageBroker;
using MessageBroker.Core.Subscriber;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MessageBroker.Core.Message;

public class MessageHandler<TMessage> : IMessageHandler<TMessage>
{
    private readonly List<object> _instances;
    private readonly BufferBlock<object> _instancesQueue;
    private readonly ILogger _logger;

    private readonly Func<TMessage, byte[]> _messagePayloadProvider;
    private readonly SubscriberRuntimeInfo _subscriberRuntimeInfo;
    protected IReadOnlyCollection<ISubscriberInvokerSettings> _invokers;

    public MessageHandler(
            SubscriberSettings subscriberSettings,
            MessageBrokerBase messageBroker,
            Func<TMessage, byte[]> messagePayloadProvider)
    {
        _logger = messageBroker.LoggerFactory
            .CreateLogger<MessageHandler<TMessage>>();

        MessageBroker = messageBroker;
        SubscriberSettings = subscriberSettings ??
                             throw new ArgumentNullException(
                                 nameof(subscriberSettings));
        _subscriberRuntimeInfo = new SubscriberRuntimeInfo(subscriberSettings);
        _messagePayloadProvider = messagePayloadProvider;
        _invokers = subscriberSettings.Invokers.ToList();

        _instancesQueue = new BufferBlock<object>();
        _instances = new List<object>
        {
            MessageBroker.Settings.ServiceProvider.GetServices(
                subscriberSettings.SubscriberType)
        };
        _instances.ForEach(x => _instancesQueue.Post(x));
    }

    public MessageBrokerBase MessageBroker { get; }
    public SubscriberSettings SubscriberSettings { get; }

    public virtual async Task HandleMessage(TMessage msg)
    {
        var msgPayload = _messagePayloadProvider(msg);
        DateTimeOffset? expires = null;

        _logger.LogDebug("Deserializing message...");
        var message =
            MessageBroker.Serializer.Deserialize(SubscriberSettings.MessageType,
                msgPayload);
        
        if (expires.HasValue)
        {
            var currentTime = MessageBroker.CurrentTime;
            if (currentTime > expires.Value)
            {
                _logger.LogWarning(
                    "The message arrived too late and is already expired (expires {ExpireTime}, current {CurrentTime})",
                    expires.Value, currentTime);

                try
                {
                    SubscriberSettings.OnMessageExpired?.Invoke(
                        SubscriberSettings, message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,"Method {MethodName} failed",
                        nameof(ISubscriberEvent.OnMessageExpired));
                }

                return;
            }
        }

        var subscriberInstances = await _instancesQueue
            .ReceiveAsync(MessageBroker.CancellationToken)
            .ConfigureAwait(false);
        try
        {
            foreach (var instance in (IEnumerable)subscriberInstances)
            {
                var task = _subscriberRuntimeInfo.OnHandle(instance, message);
                await task.ConfigureAwait(false);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Subscriber execution failed");

            try
            {
                SubscriberSettings.OnMessageFault?.Invoke(SubscriberSettings,
                    message, e);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Method {MethodName} failed",
                    nameof(ISubscriberEvent.OnMessageFault));
            }
        }
        finally
        {
            await _instancesQueue.SendAsync(subscriberInstances)
                .ConfigureAwait(false);
        }
    }
}
