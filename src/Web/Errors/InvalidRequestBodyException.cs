using ServiceStandards.Domain.Errors;

namespace ServiceStandards.Web.Errors;

/// <summary>
/// Raised when a request body is not one JSON object with known members.
/// </summary>
/// <remarks>
/// This failure lives in the Web layer because it describes the transport
/// rather than a rule. It still carries a domain error code, so a client reads
/// one vocabulary across every failure the service reports.
/// </remarks>
internal sealed class InvalidRequestBodyException : ValidationException
{
    public InvalidRequestBodyException()
        : base(ErrorConstants.CodeInvalidBody, ErrorConstants.MsgInvalidBody)
    {
    }
}
