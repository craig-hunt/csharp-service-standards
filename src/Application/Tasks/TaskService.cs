using ServiceStandards.Application.Abstractions;
using ServiceStandards.Domain.Tasks;

namespace ServiceStandards.Application.Tasks;

/// <summary>
/// The task operations an endpoint calls.
/// </summary>
/// <remarks>
/// Counting and filtering run here rather than in the store, so every store
/// implementation reports the same totals and a test can exercise the rule
/// without a database.
/// </remarks>
public sealed class TaskService(ITaskStore store)
{
    public async Task<TaskView> ListAsync(TaskFilter filter, CancellationToken cancellationToken)
    {
        var all = await store.ListAsync(cancellationToken).ConfigureAwait(false);
        return TaskView.Summarize(all, filter);
    }

    public Task<TaskItem> CreateAsync(TaskTitle title, CancellationToken cancellationToken) =>
        store.CreateAsync(title, cancellationToken);

    public Task<TaskItem> SetCompletedAsync(TaskId id, bool completed, CancellationToken cancellationToken) =>
        store.SetCompletedAsync(id, completed, cancellationToken);

    public Task DeleteAsync(TaskId id, CancellationToken cancellationToken) =>
        store.DeleteAsync(id, cancellationToken);

    public Task<long> ClearCompletedAsync(CancellationToken cancellationToken) =>
        store.DeleteCompletedAsync(cancellationToken);
}
