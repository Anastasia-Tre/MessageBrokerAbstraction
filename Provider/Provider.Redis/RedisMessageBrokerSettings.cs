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
        public string Configuration { get; set; }
        
        public Func<ConnectionMultiplexer> ConnectionFactory { get; set; }
        
        public RedisMessageBrokerSettings(string configuration)
        {
            Configuration = configuration;
            ConnectionFactory = () => ConnectionMultiplexer.Connect(Configuration);
        }
    }
}
