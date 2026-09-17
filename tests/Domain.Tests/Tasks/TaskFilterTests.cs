using ServiceStandards.Domain.Errors;
using ServiceStandards.Domain.Tasks;
using Xunit;

namespace ServiceStandards.Domain.Tests.Tasks;

/// <summary>
/// Filter parsing and membership.
/// </summary>
public sealed class TaskFilterTests
{
    private const long AnyId = 1;
    private const string AnyTitle = "Any task";

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("all")]
    public void ParseFilterTreatsAnAbsentValueAsAll(string? raw) =>
        Assert.Equal(TaskFilter.All, TaskFilterExtensions.ParseFilter(raw));

    [Fact]
    public void ParseFilterReadsActive() =>
        Assert.Equal(TaskFilter.Active, TaskFilterExtensions.ParseFilter(TaskConstants.FilterActive));

    [Fact]
    public void ParseFilterReadsCompleted() =>
        Assert.Equal(TaskFilter.Completed, TaskFilterExtensions.ParseFilter(TaskConstants.FilterCompleted));

    [Theory]
    [InlineData("ALL")]
    [InlineData("Active")]
    [InlineData("done")]
    [InlineData(" active")]
    public void ParseFilterRejectsAnythingElse(string raw) =>
        Assert.Throws<InvalidTaskFilterException>(() => TaskFilterExtensions.ParseFilter(raw));

    [Theory]
    [InlineData(TaskFilter.All, false, true)]
    [InlineData(TaskFilter.All, true, true)]
    [InlineData(TaskFilter.Active, false, true)]
    [InlineData(TaskFilter.Active, true, false)]
    [InlineData(TaskFilter.Completed, false, false)]
    [InlineData(TaskFilter.Completed, true, true)]
    public void IncludesMatchesCompletion(TaskFilter filter, bool completed, bool included)
    {
        var task = new TaskItem(TaskId.From(AnyId), TaskTitle.From(AnyTitle), completed);

        Assert.Equal(included, filter.Includes(task));
    }

    [Theory]
    [InlineData(TaskFilter.All, TaskConstants.FilterAll)]
    [InlineData(TaskFilter.Active, TaskConstants.FilterActive)]
    [InlineData(TaskFilter.Completed, TaskConstants.FilterCompleted)]
    public void ToQueryValueRoundTrips(TaskFilter filter, string expected) =>
        Assert.Equal(expected, filter.ToQueryValue());
}
