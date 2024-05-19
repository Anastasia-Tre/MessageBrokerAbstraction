using MessageBroker.Core.Message;
using MessageBroker.Core.Subscriber;
using Microsoft.Extensions.Logging;
using MQTTnet;

namespace Provider.Mqtt;

public class MqttSubscriber : SubscriberBase
{
    public IMessageHandler<MqttApplicationMessage> MessageHandler;

    public MqttSubscriber(ILogger logger, string topic,
        IMessageHandler<MqttApplicationMessage> messageHandler) : base(logger)
    {
        Topic = topic;
        MessageHandler = messageHandler;
    }

    public string Topic { get; }

    public override Action<SubscriberSettings, object> OnMessageExpired
    {
        get;
        set;
    }

    public override Action<SubscriberSettings, object, Exception> OnMessageFault
    {
        get;
        set;
    }

    protected override Task OnStart()
    {
        return Task.CompletedTask;
    }

    protected override Task OnStop()
    {
        return Task.CompletedTask;
    }
}
