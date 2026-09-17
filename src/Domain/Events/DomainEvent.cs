namespace ServiceStandards.Domain.Events;

/// <summary>
/// Something the domain recorded as having happened.
/// </summary>
/// <remarks>
/// The payload carries primitives rather than domain types. An event outlives
/// the process that raised it and may reach a consumer that shares no code with
/// this service, so its wire shape stays independent of the types in here. A
/// typed identifier protects calls inside this process; it would only couple a
/// reader outside it.
/// </remarks>
public abstract record DomainEvent(Guid EventId, DateTimeOffset OccurredAt);

/// <summary>A validated signup reached the database.</summary>
public sealed record SignupRecorded(
    Guid EventId,
    DateTimeOffset OccurredAt,
    long SignupId,
    string Email,
    string Plan,
    int Seats)
    : DomainEvent(EventId, OccurredAt);
