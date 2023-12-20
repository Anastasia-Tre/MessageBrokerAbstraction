namespace MessageBroker.Core.MessageBroker;

public abstract class MessageBrokerBase<TProviderSettings> : MessageBrokerBase 
    where TProviderSettings : class
{
    public TProviderSettings ProviderSettings { get; }

    protected MessageBrokerBase(MessageBrokerSettings settings, TProviderSettings providerSettings) : base(settings)
    {
        ProviderSettings = providerSettings ?? throw new ArgumentNullException(nameof(providerSettings));
    }
}