using ServiceStandards.Domain.Signups;

namespace ServiceStandards.Application.Abstractions;

/// <summary>
/// Records a validated signup and answers with the identifier it received.
/// </summary>
public interface ISignupStore
{
    public Task<SignupId> SaveAsync(Signup signup, CancellationToken cancellationToken);
}
