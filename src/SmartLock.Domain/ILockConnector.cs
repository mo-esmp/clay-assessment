namespace SmartLock.Domain;

public interface ILockConnector
{
    Task SendMessageAsync(string topic, object message);
}