using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessageBroker.Core.MessageBroker;

namespace Provider.RabbitMQ
{
    public class RabbitMQMessageBroker : MessageBrokerBase<RabbitMQMessageBrokerSettings>
    {
        public RabbitMQMessageBroker(MessageBrokerSettings settings, RabbitMQMessageBrokerSettings providerSettings) : base(settings, providerSettings)
        {
        }

        public override Task Start()
        {
            throw new NotImplementedException();
        }

        public override Task Stop()
        {
            throw new NotImplementedException();
        }

        public override Task PublishToProvider(Type? messageType, object message, string name,
            byte[]? payload)
        {
            throw new NotImplementedException();
        }

        public override bool IsStarted { get; set; }
    }
}
