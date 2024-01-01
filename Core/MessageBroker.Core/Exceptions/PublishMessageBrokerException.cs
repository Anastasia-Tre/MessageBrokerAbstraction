﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBroker.Core.Exceptions
{
    internal class PublishMessageBrokerException : MessageBrokerException
    {
        public PublishMessageBrokerException()
        {
        }

        public PublishMessageBrokerException(string message) : base(message)
        {
        }

        public PublishMessageBrokerException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
