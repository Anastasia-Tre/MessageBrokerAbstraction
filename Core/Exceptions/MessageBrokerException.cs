using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBroker.Core.Exceptions
{
    public class MessageBrokerException : Exception
    {
        public MessageBrokerException()
        {
        }

        public MessageBrokerException(string message) : base(message)
        {
        }

        public MessageBrokerException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
