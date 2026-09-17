using Serilog.Context;

namespace ServiceStandards.Web.Middleware;

/// <summary>
/// Gives every request one identifier and puts it on every log line the request
/// produces.
/// </summary>
/// <remarks>
/// A caller-supplied identifier survives only when it fits the length limit, so
/// a client cannot bloat every log line by sending an oversized header. The
/// identifier goes back on the response, which is how a caller reporting a
/// failure names the request an operator then finds in the logs.
/// </remarks>
internal sealed class RequestIdMiddleware(RequestDelegate next)
{
    private const string GuidFormat = "N";

    public async Task InvokeAsync(HttpContext context)
    {
        var supplied = context.Request.Headers[WebConstants.HeaderRequestId].ToString();
        var id = Accept(supplied);

        context.Response.Headers[WebConstants.HeaderRequestId] = id;
        using (LogContext.PushProperty(WebConstants.LogKeyRequestId, id))
        {
            await next(context).ConfigureAwait(false);
        }
    }

    private static string Accept(string candidate) =>
        string.IsNullOrEmpty(candidate) || candidate.Length > WebConstants.MaxRequestIdLength
            ? Guid.NewGuid().ToString(GuidFormat)
            : candidate;
}
