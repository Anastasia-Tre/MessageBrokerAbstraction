using MessageBroker.Core.Exceptions;
using MessageBroker.Core.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MessageBroker.Core.Subscriber
{
    public class SubscriberRuntimeInfo
    {
        public SubscriberSettings SubscriberSettings { get; }

        public MethodInfo SubscriberOnHandleMethod { get; }

        public PropertyInfo TaskResultProperty { get; }

        public SubscriberRuntimeInfo(SubscriberSettings subscriberSettings)
        {
            SubscriberSettings = subscriberSettings;

            if (subscriberSettings.SubscriberType == null)
            {
                throw new ConfigurationMessageBrokerException($"{nameof(subscriberSettings.SubscriberType)} is not set on the {subscriberSettings}");
            }
            if (subscriberSettings.MessageType == null)
            {
                throw new ConfigurationMessageBrokerException($"{nameof(subscriberSettings.MessageType)} is not set on the {subscriberSettings}");
            }
            SubscriberOnHandleMethod = subscriberSettings.SubscriberType.GetMethod(nameof(IMessageHandler<object>.HandleMessage), new[] { subscriberSettings.MessageType, typeof(string) });
        }

        public Task OnHandle(object subscriberInstance, object message)
        {
            return (Task)SubscriberOnHandleMethod.Invoke(subscriberInstance, new[] { message, SubscriberSettings.Topic });
        }

        public object GetResponseValue(Task task)
        {
            return TaskResultProperty.GetValue(task);
        }
    }
}
