using KnowledgeVault.Api.Events;
using System.Collections.Concurrent;

namespace KnowledgeVault.Api.Tests.Fakes;

public class FakeEventBus : IEventBus
{
    public readonly ConcurrentBag<DomainEvent> Published = new();

    public Task PublishAsync<TEvent>(TEvent @event) where TEvent : DomainEvent
    {
        Published.Add(@event);
        return Task.CompletedTask;
    }

    public void Subscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : DomainEvent { }

    public IReadOnlyCollection<T> PublishedOf<T>() where T : DomainEvent =>
        Published.OfType<T>().ToList().AsReadOnly();
}