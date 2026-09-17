using ServiceStandards.Domain.Tasks;

namespace ServiceStandards.Application.Abstractions;

/// <summary>
/// The persistence this feature calls, and nothing more.
/// </summary>
/// <remarks>
/// The interface lives beside its caller rather than beside its implementation,
/// so Infrastructure depends on Application and never the reverse. A store that
/// grows a method no feature calls has grown it for the wrong reason.
/// </remarks>
public interface ITaskStore
{
    public Task<IReadOnlyList<TaskItem>> ListAsync(CancellationToken cancellationToken);

    public Task<TaskItem> CreateAsync(TaskTitle title, CancellationToken cancellationToken);

    public Task<TaskItem> SetCompletedAsync(TaskId id, bool completed, CancellationToken cancellationToken);

    public Task DeleteAsync(TaskId id, CancellationToken cancellationToken);

    public Task<long> DeleteCompletedAsync(CancellationToken cancellationToken);
}
