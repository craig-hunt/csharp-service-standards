using ServiceStandards.Domain.Errors;
using ServiceStandards.Domain.Tasks;
using Xunit;

namespace ServiceStandards.Domain.Tests.Tasks;

/// <summary>
/// Identifier parsing, at the boundary where valid meets invalid.
/// </summary>
public sealed class TaskIdTests
{
    private const long FirstValid = 1;
    private const long Negative = -1;
    private const string FirstValidText = "1";
    private const string LargeText = "9007199254740993";
    private const long Large = 9007199254740993;

    [Fact]
    public void FromAcceptsTheFirstValidIdentifier() =>
        Assert.Equal(FirstValid, TaskId.From(FirstValid).Value);

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void FromRejectsAnythingBelowOne(long value) =>
        Assert.Throws<InvalidTaskIdException>(() => TaskId.From(value));

    [Fact]
    public void TryParseReadsAWholeNumber()
    {
        var parsed = TaskId.TryParse(FirstValidText, out var id);

        Assert.True(parsed);
        Assert.Equal(FirstValid, id.Value);
    }

    [Fact]
    public void TryParseKeepsPrecisionBeyondFiftyThreeBits()
    {
        var parsed = TaskId.TryParse(LargeText, out var id);

        Assert.True(parsed);
        Assert.Equal(Large, id.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("1.5")]
    [InlineData("one")]
    [InlineData(" 1")]
    public void TryParseRejectsAnythingElse(string? raw)
    {
        var parsed = TaskId.TryParse(raw, out var id);

        Assert.False(parsed);
        Assert.Equal(default, id);
    }

    [Fact]
    public void FromRejectsANegativeIdentifier() =>
        Assert.Throws<InvalidTaskIdException>(() => TaskId.From(Negative));
}
