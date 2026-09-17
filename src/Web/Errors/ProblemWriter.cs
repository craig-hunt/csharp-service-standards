using System.Text.Json;
using ServiceStandards.Domain.Errors;
using ServiceStandards.Web.Json;

namespace ServiceStandards.Web.Errors;

/// <summary>
/// Writes every failure in one shape.
/// </summary>
/// <remarks>
/// The body follows RFC 9457, which the sibling Go project does not: it answers
/// with a bare code, message, and fields object. The machine-readable parts
/// survive as the code and fields extension members, so a client keeps matching
/// on a stable code while the envelope gains the media type and the status that
/// the standard defines. That divergence is deliberate and documented.
/// </remarks>
internal static class ProblemWriter
{
    private const int NoFields = 0;

    public static Task WriteAsync(HttpContext context, int status, string title, DomainException failure) =>
        WriteAsync(context, status, title, failure.Code, failure.Message, failure.Fields);

    public static async Task WriteAsync(
        HttpContext context,
        int status,
        string title,
        string code,
        string detail,
        IReadOnlyDictionary<string, string>? fields = null)
    {
        context.Response.StatusCode = status;
        context.Response.ContentType = WebConstants.ContentTypeProblem;

        var problem = new Dictionary<string, object>
        {
            [ProblemMembers.Type] = WebConstants.ProblemTypeBlank,
            [ProblemMembers.Title] = title,
            [ProblemMembers.Status] = status,
            [ProblemMembers.Detail] = detail,
            [WebConstants.ExtensionCode] = code,
        };

        if (fields is not null && fields.Count > NoFields)
        {
            problem[WebConstants.ExtensionFields] = fields;
        }

        await JsonSerializer
            .SerializeAsync(context.Response.Body, problem, JsonBody.Options, context.RequestAborted)
            .ConfigureAwait(false);
    }
}

/// <summary>
/// The member names RFC 9457 defines.
/// </summary>
internal static class ProblemMembers
{
    public const string Type = "type";
    public const string Title = "title";
    public const string Status = "status";
    public const string Detail = "detail";
}
