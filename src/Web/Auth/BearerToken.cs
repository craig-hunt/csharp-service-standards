namespace ServiceStandards.Web.Auth;

/// <summary>
/// Reads the credential out of an Authorization header and says whether it
/// carries the shape of a JSON Web Token.
/// </summary>
/// <remarks>
/// Counting periods across the whole header misroutes a static token that
/// happens to hold two of them: the request goes to the JWT scheme and can
/// never authenticate. This reads the scheme first and inspects only the
/// credential, and the composition root refuses a static token containing a
/// period, so the two shapes cannot overlap rather than merely being unlikely
/// to.
/// </remarks>
internal static class BearerToken
{
    private const int Empty = 0;

    public static bool LooksLikeJsonWebToken(HttpContext context)
    {
        var header = context.Request.Headers[WebConstants.HeaderAuthorization].ToString();
        if (!header.StartsWith(WebConstants.BearerPrefix, StringComparison.Ordinal))
        {
            return false;
        }

        var credential = header[WebConstants.BearerPrefix.Length..];
        var segments = credential.Split(WebConstants.JwtSeparator);

        return segments.Length == WebConstants.JwtSegmentCount
            && Array.TrueForAll(segments, segment => segment.Length > Empty);
    }
}
