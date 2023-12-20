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
        public static MessageBrokerBuilder WithProviderRedis(this MessageBrokerBuilder mbb, RedisMessageBrokerSettings redisSettings)
        {
            return mbb.WithProvider(settings => new RedisMessageBroker(settings, redisSettings));
        }
    }
}
