using ServiceStandards.Domain.Events;

namespace ServiceStandards.Application.Abstractions;

/// <summary>
/// Reacts to an event the service published.
/// </summary>
/// <remarks>
/// A consumer must absorb repeats. The outbox delivers at least once: a relay
/// can publish a message and fail before recording that it did, and the next
/// pass sends it again. A consumer that assumes exactly-once delivery will
/// double-charge, double-mail, or double-count the first time that happens.
/// </remarks>
public interface IEventConsumer
{
    public bool Accepts(DomainEvent domainEvent);

    public Task ConsumeAsync(DomainEvent domainEvent, CancellationToken cancellationToken);
}

/// <summary>
/// A consumer of one event type.
/// </summary>
/// <typeparam name="TEvent">The event type this consumer accepts.</typeparam>
/// <remarks>
/// The non-generic interface lets the dispatcher hold every consumer in one
/// list, and this base restores the typed signature without reflection at the
/// dispatch site.
/// </remarks>
public abstract class DomainEventConsumer<TEvent> : IEventConsumer
    where TEvent : DomainEvent
{
    public bool Accepts(DomainEvent domainEvent) => domainEvent is TEvent;

    public Task ConsumeAsync(DomainEvent domainEvent, CancellationToken cancellationToken) =>
        ConsumeAsync((TEvent)domainEvent, cancellationToken);

    protected abstract Task ConsumeAsync(TEvent domainEvent, CancellationToken cancellationToken);
}

/// <summary>
/// Delivers an event to the consumers that want it.
/// </summary>
public interface IEventDispatcher
{
    public Task DispatchAsync(DomainEvent domainEvent, CancellationToken cancellationToken);
}
