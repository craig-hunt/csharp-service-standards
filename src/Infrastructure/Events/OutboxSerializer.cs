using System.Text.Json;
using ServiceStandards.Domain.Events;
using ServiceStandards.Infrastructure.Persistence;

namespace ServiceStandards.Infrastructure.Events;

/// <summary>
/// Turns an event into an outbox row and back.
/// </summary>
/// <remarks>
/// The stored type name maps through a table rather than through
/// Type.GetType. Deserializing whatever type name a row happens to hold would
/// let anything that can write a row choose a type to construct. The map also
/// survives a namespace rename, which a stored assembly-qualified name does
/// not.
/// </remarks>
internal static class OutboxSerializer
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    private static readonly Dictionary<string, Type> Known = new(StringComparer.Ordinal)
    {
        [nameof(SignupRecorded)] = typeof(SignupRecorded),
    };

    public static OutboxRow ToRow(DomainEvent domainEvent) => new()
    {
        EventId = domainEvent.EventId,
        Type = domainEvent.GetType().Name,
        Payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), Options),
        OccurredAt = domainEvent.OccurredAt,
    };

    public static DomainEvent? FromRow(OutboxRow row) =>
        Known.TryGetValue(row.Type, out var type)
            ? JsonSerializer.Deserialize(row.Payload, type, Options) as DomainEvent
            : null;
}
