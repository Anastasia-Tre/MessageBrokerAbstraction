using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class MessageBrokerSettings
    {
        public IServiceProvider ServiceProvider;

        public Type SerializerType { get; set; }
    }
}
