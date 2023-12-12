namespace MessageBroker.Core;

public interface IMessageHandler
{
    void OnStart();
    void OnMessage();
    void OnEnd();
}