using ServiceStandards.Domain.Tasks;
using Xunit;

namespace ServiceStandards.Domain.Tests.Tasks;

/// <summary>
/// The counts a list request reports.
/// </summary>
/// <remarks>
/// Remaining and Total describe the whole set while the task list describes the
/// filtered view. Asserting them under a filter that hides rows is the only way
/// to catch an implementation that counts what it shows.
/// </remarks>
public sealed class TaskViewTests
{
    private const long FirstId = 1;
    private const long SecondId = 2;
    private const long ThirdId = 3;
    private const string FirstTitle = "Write the runbook";
    private const string SecondTitle = "Review the contract";
    private const string ThirdTitle = "File the report";
    private const int TotalTasks = 3;
    private const int RemainingTasks = 2;
    private const int ActiveShown = 2;

    [Fact]
    public void SummarizeCountsRemainingAcrossEveryTask()
    {
        var view = TaskView.Summarize(Sample(), TaskFilter.Completed);

        Assert.Equal(RemainingTasks, view.Remaining);
    }

    [Fact]
    public void SummarizeCountsTotalAcrossEveryTask()
    {
        var view = TaskView.Summarize(Sample(), TaskFilter.Active);

        Assert.Equal(TotalTasks, view.Total);
    }

    [Fact]
    public void SummarizeShowsEveryTaskWithoutAFilter()
    {
        var view = TaskView.Summarize(Sample(), TaskFilter.All);

        Assert.Equal(TotalTasks, view.Tasks.Count);
    }

    [Fact]
    public void SummarizeShowsOnlyIncompleteTasksWhenActive()
    {
        var view = TaskView.Summarize(Sample(), TaskFilter.Active);

        Assert.Equal(ActiveShown, view.Tasks.Count);
        Assert.All(view.Tasks, task => Assert.False(task.Completed));
    }

    [Fact]
    public void SummarizeShowsOnlyCompletedTasksWhenCompleted()
    {
        var view = TaskView.Summarize(Sample(), TaskFilter.Completed);

        var shown = Assert.Single(view.Tasks);
        Assert.True(shown.Completed);
    }

    [Fact]
    public void SummarizeKeepsTheOrderTheStoreReturned()
    {
        var view = TaskView.Summarize(Sample(), TaskFilter.All);

        Assert.Equal(FirstId, view.Tasks[0].Id.Value);
        Assert.Equal(ThirdId, view.Tasks[^1].Id.Value);
    }

    [Fact]
    public void SummarizeReportsNothingForAnEmptySet()
    {
        var view = TaskView.Summarize([], TaskFilter.All);

        Assert.Empty(view.Tasks);
        Assert.Equal(0, view.Remaining);
        Assert.Equal(0, view.Total);
    }

    private static IReadOnlyList<TaskItem> Sample() =>
    [
        new TaskItem(TaskId.From(FirstId), TaskTitle.From(FirstTitle), false),
        new TaskItem(TaskId.From(SecondId), TaskTitle.From(SecondTitle), true),
        new TaskItem(TaskId.From(ThirdId), TaskTitle.From(ThirdTitle), false),
    ];
}
