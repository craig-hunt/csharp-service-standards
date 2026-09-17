using ServiceStandards.Domain.Errors;
using ServiceStandards.Domain.Tasks;
using Xunit;

namespace ServiceStandards.Domain.Tests.Tasks;

/// <summary>
/// The title rules, asserted at their boundaries.
/// </summary>
/// <remarks>
/// The length cases sit exactly on the limit and one past it, because a test
/// that checks a comfortably short title and a comfortably long one passes
/// whether the comparison reads greater-than or greater-than-or-equal.
/// </remarks>
public sealed class TaskTitleTests
{
    private const string Untrimmed = "  Review the plan  ";
    private const string Trimmed = "Review the plan";
    private const char Filler = 'a';

    [Fact]
    public void FromTrimsSurroundingWhitespace()
    {
        var title = TaskTitle.From(Untrimmed);

        Assert.Equal(Trimmed, title.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FromRejectsAnAbsentTitle(string? raw) =>
        Assert.Throws<TaskTitleRequiredException>(() => TaskTitle.From(raw));

    [Fact]
    public void FromAcceptsATitleAtTheLimit()
    {
        var atLimit = new string(Filler, TaskConstants.MaxTitleLength);

        var title = TaskTitle.From(atLimit);

        Assert.Equal(TaskConstants.MaxTitleLength, title.Value.Length);
    }

    [Fact]
    public void FromRejectsATitleBeyondTheLimit()
    {
        var beyondLimit = new string(Filler, TaskConstants.MaxTitleLength + 1);

        Assert.Throws<TaskTitleTooLongException>(() => TaskTitle.From(beyondLimit));
    }
}
