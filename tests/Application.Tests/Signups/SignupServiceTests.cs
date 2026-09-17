using ServiceStandards.Application.Signups;
using ServiceStandards.Application.Tests.Fakes;
using ServiceStandards.Domain.Signups;
using Xunit;

namespace ServiceStandards.Application.Tests.Signups;

/// <summary>
/// What the signup service returns once a signup is recorded.
/// </summary>
public sealed class SignupServiceTests
{
    private const string Name = "Priya Raman";
    private const string Email = "priya@example.com";
    private const int Seats = 4;
    private const string Notes = "Rolling out to one team first.";
    private const long ExpectedId = 42;

    [Fact]
    public async Task CreateAsyncHandsTheStoreTheSignup()
    {
        var store = new FakeSignupStore();
        var service = new SignupService(store);

        await service.CreateAsync(Sample(), TestContext.Current.CancellationToken);

        Assert.Equal(Name, store.Saved?.FullName);
        Assert.Equal(Seats, store.Saved?.Seats);
    }

    [Fact]
    public async Task CreateAsyncReportsTheIdentifierTheStoreAssigned()
    {
        var service = new SignupService(new FakeSignupStore());

        var confirmation = await service.CreateAsync(Sample(), TestContext.Current.CancellationToken);

        Assert.Equal(ExpectedId, confirmation.Id.Value);
    }

    [Fact]
    public async Task CreateAsyncSummarizesTheSignup()
    {
        var service = new SignupService(new FakeSignupStore());

        var confirmation = await service.CreateAsync(Sample(), TestContext.Current.CancellationToken);

        Assert.Contains(Name, confirmation.Summary, StringComparison.Ordinal);
        Assert.Contains(SignupConstants.PlanGrowth, confirmation.Summary, StringComparison.Ordinal);
    }

    private static Signup Sample() =>
        new(Name, Email, Plan.From(SignupConstants.PlanGrowth), Seats, Notes);
}
