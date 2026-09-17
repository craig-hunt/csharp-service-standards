using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ServiceStandards.Application.Abstractions;
using ServiceStandards.Domain.Events;
using ServiceStandards.Domain.Signups;
using ServiceStandards.Infrastructure.Events;
using ServiceStandards.Infrastructure.Persistence;
using ServiceStandards.Infrastructure.Stores;
using Xunit;

namespace ServiceStandards.Integration.Tests;

/// <summary>
/// The delivery path: claim, dispatch, then mark.
/// </summary>
/// <remarks>
/// These drive the publisher directly. Reaching it through the background
/// service would make the suite wait on a timer and would leave the ordering
/// between dispatch and marking untested, which is the property that decides
/// whether a failure loses a message or repeats one.
/// </remarks>
public sealed class OutboxPublisherTests(PostgresFixture fixture)
    : IClassFixture<PostgresFixture>
{
    private const string Name = "Publisher Tester";
    private const string DeliveredEmail = "publish-one@example.com";
    private const string Notes = "Checking delivery.";
    private const int Seats = 2;
    private const string DrainedEmail = "publish-drained@example.com";
    private const string UnknownType = "SomethingNobodyRegistered";
    private const string UnknownPayload = "{}";
    private const int Nothing = 0;

    [Fact]
    public async Task PublishingDeliversTheMessageThenMarksIt()
    {
        await using var writer = fixture.CreateContext();
        var store = new EfSignupStore(writer, new FixedClock());
        await store.SaveAsync(Sample(), TestContext.Current.CancellationToken);

        var dispatcher = new RecordingDispatcher();
        await using var database = fixture.CreateContext();
        var publisher = new OutboxPublisher(
            database,
            dispatcher,
            new FixedClock(),
            NullLogger<OutboxPublisher>.Instance);

        var published = await publisher.PublishPendingAsync(TestContext.Current.CancellationToken);

        Assert.True(published > Nothing);
        Assert.Contains(
            dispatcher.Seen.OfType<SignupRecorded>(),
            recorded => recorded.Email == DeliveredEmail);

        await using var reader = fixture.CreateContext();
        var row = await reader.OutboxMessages
            .AsNoTracking()
            .SingleAsync(candidate => candidate.Payload.Contains(DeliveredEmail), TestContext.Current.CancellationToken);

        Assert.Equal(FixedClock.Moment, row.PublishedAt);
    }

    [Fact]
    public async Task AnUnresolvableTypeStaysUnpublished()
    {
        var eventId = Guid.NewGuid();

        await using var writer = fixture.CreateContext();
        writer.OutboxMessages.Add(new OutboxRow
        {
            EventId = eventId,
            Type = UnknownType,
            Payload = UnknownPayload,
            OccurredAt = FixedClock.Moment,
        });
        await writer.SaveChangesAsync(TestContext.Current.CancellationToken);

        var dispatcher = new RecordingDispatcher();
        await using var database = fixture.CreateContext();
        var publisher = new OutboxPublisher(
            database,
            dispatcher,
            new FixedClock(),
            NullLogger<OutboxPublisher>.Instance);

        await publisher.PublishPendingAsync(TestContext.Current.CancellationToken);

        await using var reader = fixture.CreateContext();
        var row = await reader.OutboxMessages
            .AsNoTracking()
            .SingleAsync(candidate => candidate.EventId == eventId, TestContext.Current.CancellationToken);

        Assert.Null(row.PublishedAt);
    }

    [Fact]
    public async Task PublishingTwiceDeliversNothingTheSecondTime()
    {
        await using var writer = fixture.CreateContext();
        var store = new EfSignupStore(writer, new FixedClock());
        await store.SaveAsync(Sample(DrainedEmail), TestContext.Current.CancellationToken);

        await using var first = fixture.CreateContext();
        await Publisher(first, new RecordingDispatcher())
            .PublishPendingAsync(TestContext.Current.CancellationToken);

        var second = new RecordingDispatcher();
        await using var database = fixture.CreateContext();

        var published = await Publisher(database, second)
            .PublishPendingAsync(TestContext.Current.CancellationToken);

        // A message already marked is never claimed again, so a second pass
        // over a drained outbox does no work rather than redelivering.
        Assert.Equal(Nothing, published);
        Assert.Empty(second.Seen);
    }

    private static Signup Sample() => Sample(DeliveredEmail);

    private static Signup Sample(string email) =>
        new(Name, email, Plan.From(SignupConstants.PlanGrowth), Seats, Notes);

    private static OutboxPublisher Publisher(ServiceStandardsDbContext database, IEventDispatcher dispatcher) =>
        new(database, dispatcher, new FixedClock(), NullLogger<OutboxPublisher>.Instance);
}
