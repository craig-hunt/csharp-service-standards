using System.Diagnostics.CodeAnalysis;
using ServiceStandards.Domain.Errors;

namespace ServiceStandards.Web.Errors;

/// <summary>
/// Turns a domain failure into the response that describes it.
/// </summary>
/// <remarks>
/// Mapping lives here so no endpoint carries a try/catch and no two endpoints
/// answer the same failure differently. A validation failure carrying field
/// problems is unprocessable content; one carrying none rejected a query value
/// and is a bad request. Anything unrecognized logs with the request identifier
/// and answers with a generic body, so internal detail never reaches a caller.
/// </remarks>
internal sealed partial class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    private const int NoFields = 0;

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "The boundary converts every unhandled failure into one generic response; a narrower catch would let an unanticipated type reach the default handler and leak internal detail.")]
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context).ConfigureAwait(false);
        }
        catch (NotFoundException failure)
        {
            await ProblemWriter
                .WriteAsync(context, StatusCodes.Status404NotFound, WebConstants.TitleNotFound, failure)
                .ConfigureAwait(false);
        }
        catch (ValidationException failure)
        {
            var unprocessable = failure.Fields.Count > NoFields;
            await ProblemWriter.WriteAsync(
                    context,
                    unprocessable ? StatusCodes.Status422UnprocessableEntity : StatusCodes.Status400BadRequest,
                    unprocessable ? WebConstants.TitleUnprocessable : WebConstants.TitleBadRequest,
                    failure)
                .ConfigureAwait(false);
        }
        catch (Exception failure)
        {
            LogRequestFailed(logger, failure);
            await ProblemWriter.WriteAsync(
                    context,
                    StatusCodes.Status500InternalServerError,
                    WebConstants.TitleInternal,
                    ErrorConstants.CodeInternal,
                    ErrorConstants.MsgInternal)
                .ConfigureAwait(false);
        }
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = WebConstants.MsgRequestFailed)]
    private static partial void LogRequestFailed(ILogger logger, Exception failure);
}
