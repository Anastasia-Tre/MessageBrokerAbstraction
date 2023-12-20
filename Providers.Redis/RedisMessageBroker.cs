using MessageBroker.Core.MessageBroker;
using StackExchange.Redis;

namespace Provider.Redis;

public class RedisMessageBroker : MessageBrokerBase<RedisMessageBrokerSettings>
{
    protected IConnectionMultiplexer Connection { get; private set; }
    protected IDatabase Database { get; private set; }

    public RedisMessageBroker(MessageBrokerSettings settings, RedisMessageBrokerSettings redisSettings) 
        : base(settings, redisSettings)
    {
        Connection = redisSettings.ConnectionFactory();
        Database = Connection.GetDatabase();
    }
}