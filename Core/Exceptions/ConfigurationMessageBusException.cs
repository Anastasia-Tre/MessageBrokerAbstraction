using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBroker.Core.Exceptions
{
    public class ConfigurationMessageBusException : MessageBrokerException
    {
        public ConfigurationMessageBusException()
        {
        }

        public ConfigurationMessageBusException(string message) : base(message)
        {
        }

        public ConfigurationMessageBusException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
