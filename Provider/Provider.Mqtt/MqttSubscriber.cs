using MQTTnet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessageBroker.Core.Message;
using MessageBroker.Core.Subscriber;
using Microsoft.Extensions.Logging;

namespace Provider.Mqtt
{
    public class MqttSubscriber : SubscriberBase
    {
        public IMessageHandler<MqttApplicationMessage> MessageHandler;
        public string Topic { get; }

        public MqttSubscriber(ILogger logger, string topic, IMessageHandler<MqttApplicationMessage> messageHandler) : base(logger)
        {
            Topic = topic;
            MessageHandler = messageHandler;
        }

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

        protected override Task OnStart() => Task.CompletedTask;

        protected override Task OnStop() => Task.CompletedTask;
    }
}
