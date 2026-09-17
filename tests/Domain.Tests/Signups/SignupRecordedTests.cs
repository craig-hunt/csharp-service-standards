using System.Globalization;
using ServiceStandards.Domain.Signups;
using Xunit;

namespace ServiceStandards.Domain.Tests.Signups;

/// <summary>
/// The event a recorded signup produces.
/// </summary>
/// <remarks>
/// The signup composes its own event, so these assert that the fact leaving the
/// service matches the signup that produced it rather than whatever the calling
/// layer chose to copy across.
/// </remarks>
public sealed class SignupRecordedTests
{
    private const string Name = "Dana Reyes";
    private const string Email = "dana@example.com";
    private const string Notes = "Migrating from a legacy system.";
    private const int Seats = 5;
    private const long StoredId = 7;
    private const string EventIdText = "11111111-2222-3333-4444-555555555555";
    private const string OccurredText = "2026-09-17T10:30:00+00:00";

    private static readonly Guid EventId = Guid.Parse(EventIdText);

    private static readonly DateTimeOffset Occurred =
        DateTimeOffset.Parse(OccurredText, CultureInfo.InvariantCulture);

    [Fact]
    public void RecordedCarriesTheStoredIdentifier()
    {
        var recorded = Sample().Recorded(SignupId.From(StoredId), EventId, Occurred);

        Assert.Equal(StoredId, recorded.SignupId);
        Assert.Equal(EventId, recorded.EventId);
    }

    [Fact]
    public void RecordedCarriesTheMomentItHappened()
    {
        var recorded = Sample().Recorded(SignupId.From(StoredId), EventId, Occurred);

        Assert.Equal(Occurred, recorded.OccurredAt);
    }

    [Fact]
    public void RecordedCarriesTheSignupDetail()
    {
        var recorded = Sample().Recorded(SignupId.From(StoredId), EventId, Occurred);

        Assert.Equal(Email, recorded.Email);
        Assert.Equal(SignupConstants.PlanGrowth, recorded.Plan);
        Assert.Equal(Seats, recorded.Seats);
    }

    private static Signup Sample() =>
        new(Name, Email, Plan.From(SignupConstants.PlanGrowth), Seats, Notes);
}
