namespace MessageBroker.Core.Exceptions;

internal class
    InvalidConfigurationMessageBrokerException : MessageBrokerException
{
    public InvalidConfigurationMessageBrokerException()
    {
    }

    public InvalidConfigurationMessageBrokerException(string message) :
        base(message)
    {
    }

    public InvalidConfigurationMessageBrokerException(string message,
        Exception innerException) : base(message, innerException)
    {
    }
}
