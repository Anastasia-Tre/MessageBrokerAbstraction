namespace MessageBroker.Core.Exceptions;

public class MessageBrokerException : Exception
{
    public MessageBrokerException()
    {
    }

    public MessageBrokerException(string message) : base(message)
    {
    }

    public MessageBrokerException(string message, Exception innerException) :
        base(message, innerException)
    {
    }
}
