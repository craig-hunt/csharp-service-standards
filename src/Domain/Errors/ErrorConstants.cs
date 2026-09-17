namespace ServiceStandards.Domain.Errors;

/// <summary>
/// The codes and messages every feature shares, as opposed to the per-feature
/// codes that live beside the feature they describe.
/// </summary>
/// <remarks>
/// A validation failure answers with one envelope code and puts the per-field
/// sentences in the fields map, so a client matches on a stable code while a
/// form renders the sentence next to the input that caused it.
/// </remarks>
public static class ErrorConstants
{
    public const string CodeInvalidBody = "invalid_body";
    public const string CodeValidation = "validation_failed";
    public const string CodeInternal = "internal_error";

    public const string MsgInvalidBody = "request body must hold one JSON object with known fields";
    public const string MsgValidation = "one or more fields need attention";
    public const string MsgInternal = "the server could not complete the request";
}
