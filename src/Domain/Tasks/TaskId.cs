using System.Globalization;
using ServiceStandards.Domain.Errors;

namespace ServiceStandards.Domain.Tasks;

/// <summary>
/// A task identifier, distinct from every other identifier in the system.
/// </summary>
/// <remarks>
/// A long would compile just as well in every position an identifier appears,
/// which is exactly the problem: passing an inventory id where a task id belongs
/// would type-check. A readonly record struct costs one allocation-free wrapper
/// and makes that mistake impossible.
/// </remarks>
public readonly record struct TaskId
{
    private const long FirstValidId = 1;

    private TaskId(long value) => Value = value;

    public long Value { get; }

    public static TaskId From(long value) =>
        value < FirstValidId
            ? throw new InvalidTaskIdException()
            : new TaskId(value);

    /// <summary>
    /// Wraps a value the database already constrains, without validating it.
    /// </summary>
    /// <remarks>
    /// EF round-trips the provider's default through a key's converter while it
    /// decides whether a key has been set, so the read direction has to accept
    /// every value the column can hold, zero included. A converter that rejects
    /// one throws before an insert ever reaches the database: a converter is a
    /// mapping, not a guard. From stays the guard for values arriving from
    /// outside, and this stays internal so nothing beyond persistence can skip
    /// it.
    /// </remarks>
    internal static TaskId FromStored(long value) => new(value);

    /// <summary>
    /// Reads an identifier from a route value.
    /// </summary>
    /// <remarks>
    /// The parse allows no surrounding whitespace and no sign, because the
    /// default number styles accept both and would let " 1" address a task. The
    /// sibling Go project rejects the same shapes, so the two agree on which
    /// requests reach a handler at all.
    /// </remarks>
    public static bool TryParse(string? raw, out TaskId id)
    {
        id = default;
        var parsed = long.TryParse(raw, NumberStyles.None, CultureInfo.InvariantCulture, out var value);
        if (!parsed || value < FirstValidId)
        {
            return false;
        }

        id = new TaskId(value);
        return true;
    }

    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}
