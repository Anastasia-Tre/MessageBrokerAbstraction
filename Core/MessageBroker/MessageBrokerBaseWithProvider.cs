using MessageBroker.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Provider.Core
{
    public abstract class MessageBrokerBase<TProviderSettings> : MessageBrokerBase 
        where TProviderSettings : class
    {
        public TProviderSettings ProviderSettings { get; }

        protected MessageBrokerBase(MessageBrokerSettings settings, TProviderSettings providerSettings) : base(settings)
        {
            ProviderSettings = providerSettings ?? throw new ArgumentNullException(nameof(providerSettings));
        }
    }
}
