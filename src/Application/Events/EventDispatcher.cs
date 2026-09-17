using ServiceStandards.Application.Abstractions;
using ServiceStandards.Domain.Events;

namespace ServiceStandards.Application.Events;

/// <summary>
/// Hands an event to every consumer that accepts it.
/// </summary>
/// <remarks>
/// This is the whole of what a mediator library would sell for this job. The
/// container already resolves a list of implementations, so dispatch costs a
/// loop and an interface rather than a dependency with its own release cadence
/// and license.
/// </remarks>
public sealed class EventDispatcher(IEnumerable<IEventConsumer> consumers)
    : IEventDispatcher
{
    public async Task DispatchAsync(DomainEvent domainEvent, CancellationToken cancellationToken)
    {
        foreach (var consumer in consumers)
        {
            if (consumer.Accepts(domainEvent))
            {
                await consumer.ConsumeAsync(domainEvent, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
