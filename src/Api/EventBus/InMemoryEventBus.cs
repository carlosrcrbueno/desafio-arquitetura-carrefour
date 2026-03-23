namespace Api.EventBus;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shared.Messaging;

/// <summary>
/// In-memory implementation of IEventBus, used as a process-local event dispatcher.
/// </summary>
public class InMemoryEventBus : IEventBus
{
    private readonly ConcurrentDictionary<Type, List<Func<object, Task>>> _handlers = new();

    public Task PublishAsync<T>(T @event)
    {
        if (@event is null)
        {
            throw new ArgumentNullException(nameof(@event));
        }

        if (_handlers.TryGetValue(typeof(T), out var handlers))
        {
            var tasks = new List<Task>(handlers.Count);
            foreach (var handler in handlers)
            {
                tasks.Add(handler(@event!));
            }

            return Task.WhenAll(tasks);
        }

        return Task.CompletedTask;
    }

    public void Subscribe<T>(Func<T, Task> handler)
    {
        if (handler is null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        var handlers = _handlers.GetOrAdd(typeof(T), _ => new List<Func<object, Task>>());
        handlers.Add(evt => handler((T)evt));
    }
}
