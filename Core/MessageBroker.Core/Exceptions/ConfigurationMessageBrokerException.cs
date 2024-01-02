namespace MessageBroker.Core.Exceptions;

public class ConfigurationMessageBrokerException : MessageBrokerException
{
    public ConfigurationMessageBrokerException()
    {
    }

    public ConfigurationMessageBrokerException(string message) : base(message)
    {
    }

    public ConfigurationMessageBrokerException(string message,
        Exception innerException) : base(message, innerException)
    {
    }
}
