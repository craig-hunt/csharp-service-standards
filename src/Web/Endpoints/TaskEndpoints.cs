using ServiceStandards.Application.Tasks;
using ServiceStandards.Domain.Errors;
using ServiceStandards.Domain.Tasks;
using ServiceStandards.Web.Contracts;
using ServiceStandards.Web.Json;

namespace ServiceStandards.Web.Endpoints;

/// <summary>
/// The task routes.
/// </summary>
/// <remarks>
/// Each handler parses, delegates, and shapes a response. No handler catches a
/// failure: parsing raises a domain failure and one middleware turns it into
/// the matching status, so the rules stay in one place.
/// </remarks>
internal static class TaskEndpoints
{
    private const string IdSegment = "/{id}";

    public static void MapTaskEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup(WebConstants.PathTasks);

        group.MapGet(string.Empty, ListAsync);
        group.MapPost(string.Empty, CreateAsync);
        group.MapDelete(string.Empty, ClearCompletedAsync);
        group.MapPatch(IdSegment, UpdateAsync);
        group.MapDelete(IdSegment, DeleteAsync);
    }

    private static async Task<IResult> ListAsync(
        HttpContext context,
        TaskService tasks,
        CancellationToken cancellationToken)
    {
        var filter = TaskFilterExtensions.ParseFilter(context.Request.Query[WebConstants.QueryFilter]);
        var view = await tasks.ListAsync(filter, cancellationToken).ConfigureAwait(false);
        return Results.Ok(TaskViewResponse.From(view));
    }

    private static async Task<IResult> CreateAsync(
        HttpContext context,
        TaskService tasks,
        CancellationToken cancellationToken)
    {
        var body = await JsonBody.ReadAsync<CreateTaskRequest>(context.Request, cancellationToken)
            .ConfigureAwait(false);

        var created = await tasks.CreateAsync(TaskTitle.From(body.Title), cancellationToken)
            .ConfigureAwait(false);

        // 201 without a Location header. A location has to address the created
        // resource, and this API defines no route that serves one task, so
        // pointing at the collection would hand a client an address that does
        // not return what it just created. The sibling Go project answers the
        // same way, so the two stay at parity.
        return Results.Json(TaskResponse.From(created), statusCode: StatusCodes.Status201Created);
    }

    private static async Task<IResult> UpdateAsync(
        HttpContext context,
        string id,
        TaskService tasks,
        CancellationToken cancellationToken)
    {
        var taskId = ParseId(id);
        var body = await JsonBody.ReadAsync<UpdateTaskRequest>(context.Request, cancellationToken)
            .ConfigureAwait(false);

        if (body.Completed is null)
        {
            throw new TaskCompletedRequiredException();
        }

        var updated = await tasks.SetCompletedAsync(taskId, body.Completed.Value, cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(TaskResponse.From(updated));
    }

    private static async Task<IResult> DeleteAsync(
        string id,
        TaskService tasks,
        CancellationToken cancellationToken)
    {
        await tasks.DeleteAsync(ParseId(id), cancellationToken).ConfigureAwait(false);
        return Results.NoContent();
    }

    private static async Task<IResult> ClearCompletedAsync(
        TaskService tasks,
        CancellationToken cancellationToken)
    {
        var removed = await tasks.ClearCompletedAsync(cancellationToken).ConfigureAwait(false);
        return Results.Ok(new ClearTasksResponse(removed));
    }

    private static TaskId ParseId(string raw) =>
        TaskId.TryParse(raw, out var id) ? id : throw new InvalidTaskIdException();
}
