namespace KnowledgeVault.Api.Events;

public class InMemoryEventBus : IEventBus
{
    private readonly Dictionary<Type, List<Delegate>> _subscribers = new();
    private readonly ILogger<InMemoryEventBus> _logger;

    public InMemoryEventBus(ILogger<InMemoryEventBus> logger)
    {
        _logger = logger;
    }

    public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : DomainEvent
    {
        var eventType = typeof(TEvent);
        _logger.LogInformation("Publishing event {EventType} with Id {EventId}", eventType.Name, @event.Id);

        if (!_subscribers.TryGetValue(eventType, out var handlers))
        {
            _logger.LogWarning("No handlers registered for event {EventType}", eventType.Name);
            return;
        }

        var tasks = handlers.Select(handler =>
        {
            var task = (Task)handler.DynamicInvoke(@event)!;
            return task;
        });

        await Task.WhenAll(tasks);
        _logger.LogInformation("Event {EventType} published to {HandlerCount} handlers", eventType.Name, handlers.Count);
    }

    public void Subscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : DomainEvent
    {
        var eventType = typeof(TEvent);
        
        if (!_subscribers.ContainsKey(eventType))
        {
            _subscribers[eventType] = new List<Delegate>();
        }

        Func<TEvent, Task> handlerDelegate = handler.HandleAsync;
        _subscribers[eventType].Add(handlerDelegate);
        
        _logger.LogInformation("Handler {HandlerType} subscribed to {EventType}", 
            handler.GetType().Name, eventType.Name);
    }
}