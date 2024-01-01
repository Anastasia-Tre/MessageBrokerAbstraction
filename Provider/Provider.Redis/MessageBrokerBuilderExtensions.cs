using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessageBroker.Core.MessageBroker;

namespace Provider.Redis
{
    public static class MessageBrokerBuilderExtensions
    {
        public static MessageBrokerBuilder WithProviderRedis(this MessageBrokerBuilder mbb, Action<RedisMessageBrokerSettings> configure)
        {
            if (mbb == null) throw new ArgumentNullException(nameof(mbb));
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            var providerSettings = new RedisMessageBrokerSettings();
            configure(providerSettings);

            return mbb.WithProvider(settings => new RedisMessageBroker(settings, providerSettings));
        }
    }
}
