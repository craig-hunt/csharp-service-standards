using ServiceStandards.Application.Tasks;
using ServiceStandards.Application.Tests.Fakes;
using ServiceStandards.Domain.Tasks;
using Xunit;

namespace ServiceStandards.Application.Tests.Tasks;

/// <summary>
/// What the task service adds over its store.
/// </summary>
/// <remarks>
/// The service exists to apply the filter and the counts, so those get the
/// assertions. The pass-through operations are checked for what they hand the
/// store, since a transposed argument is the failure that survives a shallower
/// test.
/// </remarks>
public sealed class TaskServiceTests
{
    private const long FirstId = 1;
    private const long SecondId = 2;
    private const string FirstTitle = "Draft the proposal";
    private const string SecondTitle = "Send the invoice";
    private const string NewTitle = "Book the venue";
    private const int TotalTasks = 2;
    private const int RemainingTasks = 1;
    private const long ClearedRows = 7;

    [Fact]
    public async Task ListAsyncCountsAcrossEveryTaskWhileFiltering()
    {
        var service = new TaskService(SeededStore());

        var view = await service.ListAsync(TaskFilter.Completed, TestContext.Current.CancellationToken);

        Assert.Single(view.Tasks);
        Assert.Equal(RemainingTasks, view.Remaining);
        Assert.Equal(TotalTasks, view.Total);
    }

    [Fact]
    public async Task ListAsyncShowsEveryTaskWithoutAFilter()
    {
        var service = new TaskService(SeededStore());

        var view = await service.ListAsync(TaskFilter.All, TestContext.Current.CancellationToken);

        Assert.Equal(TotalTasks, view.Tasks.Count);
    }

    [Fact]
    public async Task CreateAsyncHandsTheStoreTheTitle()
    {
        var store = new FakeTaskStore();
        var service = new TaskService(store);

        var created = await service.CreateAsync(
            TaskTitle.From(NewTitle),
            TestContext.Current.CancellationToken);

        Assert.Equal(NewTitle, store.CreatedTitle?.Value);
        Assert.Equal(NewTitle, created.Title.Value);
        Assert.False(created.Completed);
    }

    [Fact]
    public async Task SetCompletedAsyncHandsTheStoreBothArguments()
    {
        var store = SeededStore();
        var service = new TaskService(store);

        var updated = await service.SetCompletedAsync(
            TaskId.From(SecondId),
            true,
            TestContext.Current.CancellationToken);

        Assert.Equal(SecondId, store.CompletedId?.Value);
        Assert.True(store.CompletedValue);
        Assert.True(updated.Completed);
    }

    [Fact]
    public async Task SetCompletedAsyncCarriesFalseThrough()
    {
        var store = SeededStore();
        var service = new TaskService(store);

        await service.SetCompletedAsync(TaskId.From(FirstId), false, TestContext.Current.CancellationToken);

        Assert.False(store.CompletedValue);
    }

    [Fact]
    public async Task DeleteAsyncHandsTheStoreTheIdentifier()
    {
        var store = SeededStore();
        var service = new TaskService(store);

        await service.DeleteAsync(TaskId.From(FirstId), TestContext.Current.CancellationToken);

        Assert.Equal(FirstId, store.DeletedId?.Value);
    }

    [Fact]
    public async Task ClearCompletedAsyncReportsWhatTheStoreRemoved()
    {
        var store = SeededStore();
        store.ClearResult = ClearedRows;
        var service = new TaskService(store);

        var removed = await service.ClearCompletedAsync(TestContext.Current.CancellationToken);

        Assert.True(store.ClearWasCalled);
        Assert.Equal(ClearedRows, removed);
    }

    private static FakeTaskStore SeededStore()
    {
        var store = new FakeTaskStore();
        store.Seed(
            new TaskItem(TaskId.From(FirstId), TaskTitle.From(FirstTitle), false),
            new TaskItem(TaskId.From(SecondId), TaskTitle.From(SecondTitle), true));
        return store;
    }
}
