using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessageBroker.Core.Subscriber;

namespace MessageBroker.Core.Message
{
    public interface IMessageHandler< in TMessage>
    {
        SubscriberSettings SubscriberSettings { get; }

        Task HandleMessage(TMessage transportMessage);

    }
}
