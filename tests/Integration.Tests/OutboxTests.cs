using System.Globalization;
using Microsoft.EntityFrameworkCore;
using ServiceStandards.Domain.Events;
using ServiceStandards.Domain.Signups;
using ServiceStandards.Infrastructure.Stores;
using Xunit;

namespace ServiceStandards.Integration.Tests;

/// <summary>
/// The outbox, against a real database.
/// </summary>
/// <remarks>
/// Only a real transaction can show what matters here: that the signup row and
/// the message announcing it land together. A fake store would record whatever
/// the code asked it to.
/// </remarks>
public sealed class OutboxTests(PostgresFixture fixture)
    : IClassFixture<PostgresFixture>
{
    private const string Name = "Outbox Tester";
    private const string FirstEmail = "outbox-one@example.com";
    private const string SecondEmail = "outbox-two@example.com";
    private const string Notes = "Checking the outbox.";
    private const string RolledBackEmail = "outbox-rolled-back@example.com";
    private const int Seats = 4;
    private const long NoIdentifier = 0;
    private const int NoRows = 0;

    [Fact]
    public async Task SavingASignupWritesOneUnpublishedMessage()
    {
        await using var database = fixture.CreateContext();
        var store = new EfSignupStore(database, new FixedClock());

        var id = await store.SaveAsync(Sample(FirstEmail), TestContext.Current.CancellationToken);

        Assert.True(id.Value > NoIdentifier);

        await using var reader = fixture.CreateContext();
        var messages = await reader.OutboxMessages
            .AsNoTracking()
            .Where(row => row.Payload.Contains(FirstEmail))
            .ToListAsync(TestContext.Current.CancellationToken);

        var message = Assert.Single(messages);
        Assert.Equal(nameof(SignupRecorded), message.Type);
        Assert.Null(message.PublishedAt);
        Assert.Equal(FixedClock.Moment, message.OccurredAt);
    }

    [Fact]
    public async Task TheMessageCarriesTheSignupThatProducedIt()
    {
        await using var database = fixture.CreateContext();
        var store = new EfSignupStore(database, new FixedClock());

        var id = await store.SaveAsync(Sample(SecondEmail), TestContext.Current.CancellationToken);

        await using var reader = fixture.CreateContext();
        var message = await reader.OutboxMessages
            .AsNoTracking()
            .SingleAsync(row => row.Payload.Contains(SecondEmail), TestContext.Current.CancellationToken);

        Assert.Contains(SignupConstants.PlanGrowth, message.Payload, StringComparison.Ordinal);
        Assert.Contains(
            id.Value.ToString(CultureInfo.InvariantCulture),
            message.Payload,
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheSignupAndItsMessageCommitTogether()
    {
        await using var reader = fixture.CreateContext();

        var signups = await reader.Signups
            .AsNoTracking()
            .CountAsync(row => row.Email == FirstEmail, TestContext.Current.CancellationToken);
        var messages = await reader.OutboxMessages
            .AsNoTracking()
            .CountAsync(row => row.Payload.Contains(FirstEmail), TestContext.Current.CancellationToken);

        Assert.Equal(signups, messages);
    }

    [Fact]
    public async Task AFailureAfterTheSignupSaveLeavesNoRowBehind()
    {
        await using var database = fixture.CreateContext();
        var store = new EfSignupStore(database, new ThrowingClock());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            store.SaveAsync(Sample(RolledBackEmail), TestContext.Current.CancellationToken));

        await using var reader = fixture.CreateContext();
        var signups = await reader.Signups
            .AsNoTracking()
            .CountAsync(row => row.Email == RolledBackEmail, TestContext.Current.CancellationToken);
        var messages = await reader.OutboxMessages
            .AsNoTracking()
            .CountAsync(row => row.Payload.Contains(RolledBackEmail), TestContext.Current.CancellationToken);

        Assert.Equal(NoRows, signups);
        Assert.Equal(NoRows, messages);
    }

    private static Signup Sample(string email) =>
        new(Name, email, Plan.From(SignupConstants.PlanGrowth), Seats, Notes);
}
