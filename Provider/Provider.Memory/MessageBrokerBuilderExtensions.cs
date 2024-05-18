using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessageBroker.Core.MessageBroker;

namespace Provider.Memory
{
    public static class MessageBrokerBuilderExtensions
    {
        public static MemoryMessageBrokerBuilder WithProviderMemory(this MessageBrokerBuilder mbb, Action<MemoryMessageBrokerSettings> configure = null)
        {
            if (mbb is null) throw new ArgumentNullException(nameof(mbb));

            var providerSettings = new MemoryMessageBrokerSettings();
            configure?.Invoke(providerSettings);

            return new MemoryMessageBrokerBuilder(mbb.WithProvider(settings => new MemoryMessageBroker(settings, providerSettings)));
        }
    }
}
