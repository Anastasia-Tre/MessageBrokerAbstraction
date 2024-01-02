using StackExchange.Redis;

namespace Provider.Redis;

public class RedisMessageBrokerSettings
{
    public RedisMessageBrokerSettings()
    {
        ConnectionFactory =
            () => ConnectionMultiplexer.Connect(ConnectionString!);
    }

    public RedisMessageBrokerSettings(string? connectionString) : this()
    {
        ConnectionString = connectionString;
    }

    public string? ConnectionString { get; set; }

    public Func<ConnectionMultiplexer> ConnectionFactory { get; set; }
}
