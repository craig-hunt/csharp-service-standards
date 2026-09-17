using ServiceStandards.Application.Abstractions;
using ServiceStandards.Domain.Signups;

namespace ServiceStandards.Application.Signups;

/// <summary>
/// Records a signup that already passed validation.
/// </summary>
/// <remarks>
/// Validation stays in the domain and runs before this call, so the service
/// cannot receive a signup with a missing email or an unknown plan.
/// </remarks>
public sealed class SignupService(ISignupStore store)
{
    public async Task<SignupConfirmation> CreateAsync(Signup signup, CancellationToken cancellationToken)
    {
        var id = await store.SaveAsync(signup, cancellationToken).ConfigureAwait(false);
        return new SignupConfirmation(id, signup.Summary());
    }
}
