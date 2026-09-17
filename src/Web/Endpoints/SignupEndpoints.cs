using ServiceStandards.Application.Signups;
using ServiceStandards.Domain.Errors;
using ServiceStandards.Domain.Signups;
using ServiceStandards.Web.Contracts;
using ServiceStandards.Web.Json;

namespace ServiceStandards.Web.Endpoints;

/// <summary>
/// The signup route.
/// </summary>
internal static class SignupEndpoints
{
    public static void MapSignupEndpoints(this IEndpointRouteBuilder routes) =>
        routes.MapPost(WebConstants.PathSignups, CreateAsync);

    private static async Task<IResult> CreateAsync(
        HttpContext context,
        SignupService signups,
        CancellationToken cancellationToken)
    {
        var body = await JsonBody.ReadAsync<SignupRequest>(context.Request, cancellationToken)
            .ConfigureAwait(false);

        var validation = SignupValidator.Validate(body);
        if (validation.Value is null)
        {
            throw new SignupInvalidException(validation.Problems);
        }

        var confirmation = await signups.CreateAsync(validation.Value, cancellationToken)
            .ConfigureAwait(false);

        return Results.Created(WebConstants.PathSignups, SignupResponse.From(confirmation));
    }
}
