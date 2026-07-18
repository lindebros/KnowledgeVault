namespace KnowledgeVault.Api.Events;

public class InMemoryEventBus : IEventBus
{
    private readonly Dictionary<Type, List<Delegate>> _subscribers = new();
    private readonly ILogger<InMemoryEventBus> _logger;
    private readonly IServiceProvider _serviceProvider;

    public InMemoryEventBus(ILogger<InMemoryEventBus> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : DomainEvent
    {
        var eventType = typeof(TEvent);
        _logger.LogInformation("Publishing event {EventType} with Id {EventId}", eventType.Name, @event.Id);

        if (!_subscribers.TryGetValue(eventType, out var handlers))
        {
            _logger.LogDebug("No handlers for {EventType}", eventType.Name);
            return;
        }

        var tasks = handlers.Select(d => (Task)d.DynamicInvoke(@event)!);
        await Task.WhenAll(tasks);
        _logger.LogInformation("Event {EventType} published to {HandlerCount} handlers", eventType.Name, handlers.Count);
    }

    public void Subscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : DomainEvent
    {
        var eventType = typeof(TEvent);
        if (!_subscribers.TryGetValue(eventType, out var list))
        {
            list = new List<Delegate>();
            _subscribers[eventType] = list;
        }

        var handlerType = handler.GetType();

        // wrapper resolves a fresh handler from DI per publish so scoped deps are correct
        Func<TEvent, Task> wrapper = async (e) =>
        {
            using var scope = _serviceProvider.CreateScope();
            var resolved = (IEventHandler<TEvent>)scope.ServiceProvider.GetRequiredService(handlerType);
            await resolved.HandleAsync(e);
        };

        list.Add(wrapper);
        _logger.LogInformation("Handler {HandlerType} subscribed to {EventType}", handlerType.Name, eventType.Name);
    }
}