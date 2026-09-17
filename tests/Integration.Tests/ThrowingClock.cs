using ServiceStandards.Application.Abstractions;

namespace ServiceStandards.Integration.Tests;

/// <summary>
/// A clock that refuses to answer.
/// </summary>
/// <remarks>
/// The store reads the clock after it has already written the signup row and
/// before it writes the outbox row. Failing there is the one injection point
/// that proves the two writes share a transaction: the signup row exists inside
/// it, and must not survive the rollback.
/// </remarks>
internal sealed class ThrowingClock : IClock
{
    public const string Failure = "the clock refused";

    public DateTimeOffset UtcNow => throw new InvalidOperationException(Failure);
}

/// <summary>Records what it was asked to dispatch.</summary>
internal sealed class RecordingDispatcher : IEventDispatcher
{
    private readonly List<Domain.Events.DomainEvent> _seen = [];

    public IReadOnlyList<Domain.Events.DomainEvent> Seen => _seen;

    public Task DispatchAsync(Domain.Events.DomainEvent domainEvent, CancellationToken cancellationToken)
    {
        _seen.Add(domainEvent);
        return Task.CompletedTask;
    }
}
