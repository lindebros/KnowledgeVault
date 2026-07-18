namespace KnowledgeVault.Api.Events;

public interface IEventHandler<in TEvent> where TEvent : DomainEvent
{
    Task HandleAsync(TEvent @event);
}