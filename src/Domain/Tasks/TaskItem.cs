using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ServiceStandards.Domain.Tasks;

/// <summary>
/// One task, as the domain understands it.
/// </summary>
public sealed record TaskItem(TaskId Id, TaskTitle Title, bool Completed);

/// <summary>
/// What a list request answers: the tasks the filter shows, how many remain
/// incomplete across every task, and how many tasks exist.
/// </summary>
/// <remarks>
/// Remaining and Total count the whole set rather than the filtered view, so the
/// counts stay steady while a reader switches filters.
/// </remarks>
public sealed record TaskView(IReadOnlyList<TaskItem> Tasks, int Remaining, int Total)
{
    public static TaskView Summarize(IReadOnlyList<TaskItem> all, TaskFilter filter)
    {
        var shown = new List<TaskItem>(all.Count);
        var remaining = 0;
        foreach (var task in all)
        {
            if (!task.Completed)
            {
                remaining++;
            }

            if (filter.Includes(task))
            {
                shown.Add(task);
            }
        }

        return new TaskView(new ReadOnlyCollection<TaskItem>(shown), remaining, all.Count);
    }
}
