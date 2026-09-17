using ServiceStandards.Application.Health;
using ServiceStandards.Domain.Health;

namespace ServiceStandards.Web.Endpoints;

/// <summary>
/// The liveness and readiness probes.
/// </summary>
/// <remarks>
/// Both sit outside the authenticated area, because a platform probes them
/// before it has any credential and must be able to tell a stopped instance
/// from an unauthorized one.
/// </remarks>
internal static partial class HealthEndpoints
{
    public static void MapHealthEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGet(WebConstants.PathHealthLive, Live);
        routes.MapGet(WebConstants.PathHealthReady, ReadyAsync);
    }

    private static IResult Live() => Results.Ok(new HealthReport(HealthConstants.StatusOk));

    private static async Task<IResult> ReadyAsync(
        HealthService health,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var check = await health.CheckAsync(cancellationToken).ConfigureAwait(false);
        if (check.Ready)
        {
            return Results.Ok(new HealthReport(HealthConstants.StatusOk));
        }

        LogNotReady(loggerFactory.CreateLogger(typeof(HealthEndpoints)), check.Failure);

        return Results.Json(
            new HealthReport(HealthConstants.StatusUnavailable),
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }

    [LoggerMessage(EventId = 2, Level = LogLevel.Warning, Message = WebConstants.MsgNotReady)]
    private static partial void LogNotReady(ILogger logger, Exception? failure);
}
