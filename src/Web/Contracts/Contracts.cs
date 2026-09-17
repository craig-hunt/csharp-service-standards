using ServiceStandards.Domain.Inventory;
using ServiceStandards.Domain.Signups;
using ServiceStandards.Domain.Tasks;

namespace ServiceStandards.Web.Contracts;

/// <summary>What a client sends to create a task.</summary>
internal sealed record CreateTaskRequest(string? Title);

/// <summary>
/// What a client sends to change a task's completion.
/// </summary>
/// <remarks>
/// The flag is nullable so an omitted member reads as missing rather than as
/// false, which would silently reopen a completed task.
/// </remarks>
internal sealed record UpdateTaskRequest(bool? Completed);

/// <summary>One task, as a client sees it.</summary>
internal sealed record TaskResponse(TaskId Id, TaskTitle Title, bool Completed)
{
    public static TaskResponse From(TaskItem task) => new(task.Id, task.Title, task.Completed);
}

/// <summary>A filtered task list with counts over the whole set.</summary>
internal sealed record TaskViewResponse(IReadOnlyList<TaskResponse> Tasks, int Remaining, int Total)
{
    public static TaskViewResponse From(TaskView view) =>
        new([.. view.Tasks.Select(TaskResponse.From)], view.Remaining, view.Total);
}

/// <summary>How many tasks a clear-completed request removed.</summary>
internal sealed record ClearTasksResponse(long Removed);

/// <summary>What a signup confirmation returns.</summary>
internal sealed record SignupResponse(SignupId Id, string Summary)
{
    public static SignupResponse From(SignupConfirmation confirmation) =>
        new(confirmation.Id, confirmation.Summary);
}

/// <summary>One stock row, as a client sees it.</summary>
internal sealed record InventoryItemResponse(string Name, int Quantity, Status Status)
{
    public static InventoryItemResponse From(InventoryItem item) =>
        new(item.Name, item.Quantity, item.Status);
}

/// <summary>Matching stock rows with counts.</summary>
internal sealed record InventoryResponse(IReadOnlyList<InventoryItemResponse> Items, int Shown, int Total)
{
    public static InventoryResponse From(InventoryResult result) =>
        new([.. result.Items.Select(InventoryItemResponse.From)], result.Shown, result.Total);
}
