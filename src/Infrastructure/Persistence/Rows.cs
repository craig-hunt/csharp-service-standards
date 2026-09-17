using ServiceStandards.Domain.Signups;
using ServiceStandards.Domain.Tasks;

namespace ServiceStandards.Infrastructure.Persistence;

/// <summary>
/// The persistence shape of a task row.
/// </summary>
/// <remarks>
/// A row type sits between the table and the domain because the two answer
/// different questions. A row carries whatever the column holds; a domain type
/// carries only values the rules accept, and it cannot be half-built while an
/// insert waits for a generated identifier.
/// </remarks>
public sealed class TaskRow
{
    public TaskId Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public bool Completed { get; set; }
}

/// <summary>
/// The persistence shape of a signup row.
/// </summary>
public sealed class SignupRow
{
    public SignupId Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Plan { get; set; } = string.Empty;

    public int Seats { get; set; }

    public string Notes { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
}

/// <summary>
/// The persistence shape of an outbox message.
/// </summary>
/// <remarks>
/// The row commits in the same transaction as the state change that produced
/// it. That single fact is what makes the outbox worth having: without it the
/// service writes to the database and to a broker separately, and any failure
/// between the two leaves one of them wrong.
/// </remarks>
public sealed class OutboxRow
{
    public Guid EventId { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Payload { get; set; } = string.Empty;

    public DateTimeOffset OccurredAt { get; set; }

    public DateTimeOffset? PublishedAt { get; set; }
}

/// <summary>
/// The persistence shape of a stock row.
/// </summary>
public sealed class InventoryRow
{
    public string Name { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public string Status { get; set; } = string.Empty;
}
