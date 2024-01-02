using System.Reflection;
using MessageBroker.Core.Exceptions;

namespace MessageBroker.Core.Subscriber;

public class SubscriberRuntimeInfo
{
    public SubscriberRuntimeInfo(SubscriberSettings subscriberSettings)
    {
        SubscriberSettings = subscriberSettings;

        if (subscriberSettings.SubscriberType == null)
            throw new ConfigurationMessageBrokerException(
                $"{nameof(subscriberSettings.SubscriberType)} is not set on the {subscriberSettings}");

        if (subscriberSettings.MessageType == null)
            throw new ConfigurationMessageBrokerException(
                $"{nameof(subscriberSettings.MessageType)} is not set on the {subscriberSettings}");

        SubscriberOnHandleMethod = subscriberSettings.SubscriberType.GetMethod(
            nameof(ISubscriber<object>.OnHandle),
            new[] { subscriberSettings.MessageType })!;
    }

    public SubscriberSettings SubscriberSettings { get; }

    public MethodInfo SubscriberOnHandleMethod { get; }

    public PropertyInfo TaskResultProperty { get; }

    public Task OnHandle(object subscriberInstance, object message)
    {
        return (Task)SubscriberOnHandleMethod.Invoke(subscriberInstance,
            new[] { message })!;
    }

    public object GetResponseValue(Task task)
    {
        return TaskResultProperty.GetValue(task)!;
    }
}
