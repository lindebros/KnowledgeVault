namespace KnowledgeVault.Api.Events;

public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event) where TEvent : DomainEvent;
    void Subscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : DomainEvent;
}