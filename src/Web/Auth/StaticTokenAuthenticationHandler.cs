using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using ServiceStandards.Web.Errors;

namespace ServiceStandards.Web.Auth;

/// <summary>
/// Accepts one configured bearer token.
/// </summary>
/// <remarks>
/// The comparison runs in fixed time, so response timing reveals nothing about
/// how much of a guessed token matched. A length-sensitive comparison would let
/// an attacker recover the token one character at a time.
/// </remarks>
internal sealed class StaticTokenAuthenticationHandler(
    IOptionsMonitor<StaticTokenOptions> options,
    ILoggerFactory loggerFactory,
    UrlEncoder encoder)
    : AuthenticationHandler<StaticTokenOptions>(options, loggerFactory, encoder)
{
    private const string IdentityName = "static-token";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var presented = Encoding.UTF8.GetBytes(
            Request.Headers[WebConstants.HeaderAuthorization].ToString());
        var expected = Encoding.UTF8.GetBytes(WebConstants.BearerPrefix + Options.Token);

        if (!CryptographicOperations.FixedTimeEquals(presented, expected))
        {
            return Task.FromResult(AuthenticateResult.Fail(WebConstants.MsgUnauthorized));
        }

        var identity = new ClaimsIdentity(Scheme.Name);
        identity.AddClaim(new Claim(ClaimTypes.Name, IdentityName));
        var principal = new ClaimsPrincipal(identity);
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name)));
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.Headers[WebConstants.HeaderWwwAuthenticate] = WebConstants.SchemeBearer;
        await ProblemWriter.WriteAsync(
                Context,
                StatusCodes.Status401Unauthorized,
                WebConstants.TitleUnauthorized,
                WebConstants.CodeUnauthorized,
                WebConstants.MsgUnauthorized)
            .ConfigureAwait(false);
    }
}

/// <summary>
/// The token the static scheme accepts.
/// </summary>
internal sealed class StaticTokenOptions : AuthenticationSchemeOptions
{
    public string Token { get; set; } = string.Empty;
}
