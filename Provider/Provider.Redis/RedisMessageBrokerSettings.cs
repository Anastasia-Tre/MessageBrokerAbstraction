using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessageBroker.Core;
using StackExchange.Redis;

namespace Provider.Redis
{
    public class RedisMessageBrokerSettings
    {
        public string ConnectionString { get; set; }
        
        public Func<ConnectionMultiplexer> ConnectionFactory { get; set; }
        
        public RedisMessageBrokerSettings()
        {
            ConnectionFactory = () => ConnectionMultiplexer.Connect(ConnectionString);
        }

        public RedisMessageBrokerSettings(string connectionString) : this()
        {
            ConnectionString = connectionString;
        }
    }
}
