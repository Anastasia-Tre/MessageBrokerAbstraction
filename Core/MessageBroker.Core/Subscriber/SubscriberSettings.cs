﻿using MessageBroker.Core.MessageBroker;

namespace MessageBroker.Core.Subscriber;

public interface ISubscriberInvokerSettings
{
    Type MessageType { get; }

    Type SubscriberType { get; }

    Func<object, object, Task> SubscriberMethod { get; set; }
}

public class SubscriberSettings : BaseSettings, ISubscriberEvent,
    ISubscriberInvokerSettings
{
    internal SubscriberSettings()
    {
    }

    public string? Topic { get; set; }

    public ISet<ISubscriberInvokerSettings> Invokers { get; } =
        new HashSet<ISubscriberInvokerSettings>();

    public Action<SubscriberSettings, object> OnMessageExpired { get; set; }

    public Action<SubscriberSettings, object, Exception> OnMessageFault
    {
        get;
        set;
    }

    public Type SubscriberType { get; set; }
    public Func<object, object, Task> SubscriberMethod { get; set; }
    public Type MessageType { get; set; }
}
