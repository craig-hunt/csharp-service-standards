using Microsoft.EntityFrameworkCore;
using ServiceStandards.Application.Abstractions;
using ServiceStandards.Domain.Errors;
using ServiceStandards.Domain.Tasks;
using ServiceStandards.Infrastructure.Persistence;

namespace ServiceStandards.Infrastructure.Stores;

/// <summary>
/// Reads and writes tasks through EF Core.
/// </summary>
/// <remarks>
/// A row that does not exist raises the domain's not-found failure rather than
/// returning quietly, so a delete against a stale identifier answers 404 rather
/// than reporting success for work it never did.
/// </remarks>
public sealed class EfTaskStore(ServiceStandardsDbContext database)
    : ITaskStore
{
    private const int NoRows = 0;

    public async Task<IReadOnlyList<TaskItem>> ListAsync(CancellationToken cancellationToken)
    {
        var rows = await database.Tasks
            .AsNoTracking()
            .OrderBy(row => row.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows.ConvertAll(Map);
    }

    public async Task<TaskItem> CreateAsync(TaskTitle title, CancellationToken cancellationToken)
    {
        var row = new TaskRow { Title = title.Value };
        database.Tasks.Add(row);
        await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Map(row);
    }

    public async Task<TaskItem> SetCompletedAsync(TaskId id, bool completed, CancellationToken cancellationToken)
    {
        var row = await database.Tasks
            .FirstOrDefaultAsync(candidate => candidate.Id == id, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new TaskNotFoundException();

        row.Completed = completed;
        await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Map(row);
    }

    public async Task DeleteAsync(TaskId id, CancellationToken cancellationToken)
    {
        var removed = await database.Tasks
            .Where(candidate => candidate.Id == id)
            .ExecuteDeleteAsync(cancellationToken)
            .ConfigureAwait(false);

        if (removed == NoRows)
        {
            throw new TaskNotFoundException();
        }
    }

    public async Task<long> DeleteCompletedAsync(CancellationToken cancellationToken) =>
        await database.Tasks
            .Where(candidate => candidate.Completed)
            .ExecuteDeleteAsync(cancellationToken)
            .ConfigureAwait(false);

    private static TaskItem Map(TaskRow row) =>
        new(row.Id, TaskTitle.From(row.Title), row.Completed);
}
