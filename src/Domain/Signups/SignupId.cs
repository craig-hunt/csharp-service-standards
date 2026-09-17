using ServiceStandards.Domain.Errors;

namespace ServiceStandards.Domain.Signups;

/// <summary>
/// A signup identifier, distinct from every other identifier in the system.
/// </summary>
public readonly record struct SignupId
{
    private const long FirstValidId = 1;

    private SignupId(long value) => Value = value;

    public long Value { get; }

    public static SignupId From(long value) =>
        value < FirstValidId
            ? throw new InvalidSignupIdException()
            : new SignupId(value);

    /// <summary>
    /// Wraps a value the database already constrains, without validating it.
    /// </summary>
    /// <remarks>
    /// The read direction of an EF converter must accept every value the column
    /// can hold, including the provider default EF uses as its unset-key
    /// sentinel. See <see cref="Tasks.TaskId.FromStored"/> for the mechanism.
    /// </remarks>
    internal static SignupId FromStored(long value) => new(value);

    public override string ToString() => Value.ToString();
}
