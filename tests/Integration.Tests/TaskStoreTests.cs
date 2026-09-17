using Microsoft.EntityFrameworkCore;
using ServiceStandards.Domain.Errors;
using ServiceStandards.Domain.Tasks;
using ServiceStandards.Infrastructure.Persistence;
using ServiceStandards.Infrastructure.Stores;
using Xunit;

namespace ServiceStandards.Integration.Tests;

/// <summary>
/// The task store against a real PostgreSQL.
/// </summary>
/// <remarks>
/// These cover what only a database can answer: that the identity column hands
/// back a generated identifier, that a missing row raises not-found rather than
/// passing silently, and that the ordering is the one the store promises.
/// </remarks>
public sealed class TaskStoreTests(PostgresFixture fixture)
    : IClassFixture<PostgresFixture>
{
    private const string FirstTitle = "Integration first";
    private const string SecondTitle = "Integration second";
    private const long MissingId = 999999;
    private const long NoIdentifier = 0;
    private const int NoRows = 0;

    [Fact]
    public async Task CreateAssignsAGeneratedIdentifier()
    {
        await using var database = fixture.CreateContext();
        var store = new EfTaskStore(database);

        var created = await store.CreateAsync(
            TaskTitle.From(FirstTitle),
            TestContext.Current.CancellationToken);

        Assert.True(created.Id.Value > NoIdentifier);
        Assert.Equal(FirstTitle, created.Title.Value);
        Assert.False(created.Completed);
    }

    [Fact]
    public async Task ListReturnsRowsInIdentifierOrder()
    {
        await using var database = fixture.CreateContext();
        var store = new EfTaskStore(database);

        var first = await store.CreateAsync(
            TaskTitle.From(FirstTitle),
            TestContext.Current.CancellationToken);
        var second = await store.CreateAsync(
            TaskTitle.From(SecondTitle),
            TestContext.Current.CancellationToken);

        var listed = await store.ListAsync(TestContext.Current.CancellationToken);
        var positions = listed.Select(task => task.Id.Value).ToList();

        Assert.True(positions.IndexOf(first.Id.Value) < positions.IndexOf(second.Id.Value));
    }

    [Fact]
    public async Task SetCompletedPersistsTheChange()
    {
        await using var database = fixture.CreateContext();
        var store = new EfTaskStore(database);

        var created = await store.CreateAsync(
            TaskTitle.From(FirstTitle),
            TestContext.Current.CancellationToken);

        await store.SetCompletedAsync(created.Id, true, TestContext.Current.CancellationToken);

        await using var reader = fixture.CreateContext();
        var stored = await reader.Tasks
            .AsNoTracking()
            .FirstAsync(row => row.Id == created.Id, TestContext.Current.CancellationToken);

        Assert.True(stored.Completed);
    }

    [Fact]
    public async Task SetCompletedRaisesNotFoundForAMissingRow()
    {
        await using var database = fixture.CreateContext();
        var store = new EfTaskStore(database);

        await Assert.ThrowsAsync<TaskNotFoundException>(() =>
            store.SetCompletedAsync(TaskId.From(MissingId), true, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task DeleteRaisesNotFoundForAMissingRow()
    {
        await using var database = fixture.CreateContext();
        var store = new EfTaskStore(database);

        await Assert.ThrowsAsync<TaskNotFoundException>(() =>
            store.DeleteAsync(TaskId.From(MissingId), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task DeleteRemovesTheRow()
    {
        await using var database = fixture.CreateContext();
        var store = new EfTaskStore(database);

        var created = await store.CreateAsync(
            TaskTitle.From(FirstTitle),
            TestContext.Current.CancellationToken);

        await store.DeleteAsync(created.Id, TestContext.Current.CancellationToken);

        await using var reader = fixture.CreateContext();
        var remaining = await reader.Tasks
            .CountAsync(row => row.Id == created.Id, TestContext.Current.CancellationToken);

        Assert.Equal(NoRows, remaining);
    }

    [Fact]
    public async Task ClearCompletedRemovesOnlyCompletedRows()
    {
        await using var database = fixture.CreateContext();
        var store = new EfTaskStore(database);

        var active = await store.CreateAsync(
            TaskTitle.From(FirstTitle),
            TestContext.Current.CancellationToken);
        var finished = await store.CreateAsync(
            TaskTitle.From(SecondTitle),
            TestContext.Current.CancellationToken);
        await store.SetCompletedAsync(finished.Id, true, TestContext.Current.CancellationToken);

        await store.DeleteCompletedAsync(TestContext.Current.CancellationToken);

        await using var reader = fixture.CreateContext();
        var survivors = await reader.Tasks
            .AsNoTracking()
            .Select(row => row.Id)
            .ToListAsync(TestContext.Current.CancellationToken);

        Assert.Contains(active.Id, survivors);
        Assert.DoesNotContain(finished.Id, survivors);
    }
}
