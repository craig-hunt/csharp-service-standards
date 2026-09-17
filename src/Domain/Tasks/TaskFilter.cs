using ServiceStandards.Domain.Errors;

namespace ServiceStandards.Domain.Tasks;

/// <summary>
/// Which tasks a list request shows.
/// </summary>
public enum TaskFilter
{
    All,
    Active,
    Completed,
}

public static class TaskFilterExtensions
{
    /// <summary>
    /// Parses the query value, treating an absent value as <see cref="TaskFilter.All"/>.
    /// </summary>
    public static TaskFilter ParseFilter(string? raw)
    {
        if (string.IsNullOrEmpty(raw))
        {
            return TaskFilter.All;
        }

        return raw switch
        {
            TaskConstants.FilterAll => TaskFilter.All,
            TaskConstants.FilterActive => TaskFilter.Active,
            TaskConstants.FilterCompleted => TaskFilter.Completed,
            _ => throw new InvalidTaskFilterException(),
        };
    }

    public static bool Includes(this TaskFilter filter, TaskItem task) => filter switch
    {
        TaskFilter.Active => !task.Completed,
        TaskFilter.Completed => task.Completed,
        _ => true,
    };

    public static string ToQueryValue(this TaskFilter filter) => filter switch
    {
        TaskFilter.Active => TaskConstants.FilterActive,
        TaskFilter.Completed => TaskConstants.FilterCompleted,
        _ => TaskConstants.FilterAll,
    };
}
