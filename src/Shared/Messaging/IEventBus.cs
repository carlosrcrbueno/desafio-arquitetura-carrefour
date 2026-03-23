namespace Shared.Messaging;

public interface IEventBus
{
    Task PublishAsync<T>(T @event);
    void Subscribe<T>(Func<T, Task> handler);
}
