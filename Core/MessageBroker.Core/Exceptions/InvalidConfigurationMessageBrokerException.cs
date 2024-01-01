using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBroker.Core.Exceptions
{
    internal class InvalidConfigurationMessageBrokerException : MessageBrokerException
    {
        public InvalidConfigurationMessageBrokerException()
        {
        }

        public InvalidConfigurationMessageBrokerException(string message) : base(message)
        {
        }

        public InvalidConfigurationMessageBrokerException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
