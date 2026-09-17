using ServiceStandards.Domain.Errors;
using ServiceStandards.Domain.Text;

namespace ServiceStandards.Domain.Tasks;

/// <summary>
/// A validated task title: trimmed, present, and within the length limit.
/// </summary>
/// <remarks>
/// The factory counts runes rather than UTF-16 code units, so the limit lands
/// in the same place as the sibling Go project. A string field would let an
/// unvalidated value reach the database; this type cannot exist in an invalid
/// state.
/// </remarks>
public readonly record struct TaskTitle
{
    private TaskTitle(string value) => Value = value;

    public string Value { get; }

    public static TaskTitle From(string? raw)
    {
        var trimmed = (raw ?? string.Empty).Trim();
        if (trimmed.Length == 0)
        {
            throw new TaskTitleRequiredException();
        }

        if (TextLength.CountRunes(trimmed) > TaskConstants.MaxTitleLength)
        {
            throw new TaskTitleTooLongException();
        }

        return new TaskTitle(trimmed);
    }

    public override string ToString() => Value;
}
