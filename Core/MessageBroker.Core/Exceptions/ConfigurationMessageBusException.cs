namespace MessageBroker.Core.Exceptions;

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