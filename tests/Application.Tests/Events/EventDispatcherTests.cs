using ServiceStandards.Application.Abstractions;
using ServiceStandards.Application.Events;
using ServiceStandards.Domain.Events;
using Xunit;

namespace ServiceStandards.Application.Tests.Events;

/// <summary>
/// Dispatch, which is all a mediator library would have supplied here.
/// </summary>
public sealed class EventDispatcherTests
{
    private const string Email = "dana@example.com";
    private const string Plan = "Growth";
    private const int Seats = 3;
    private const long SignupId = 9;
    private const int Once = 1;
    private const int Never = 0;

    [Fact]
    public async Task DispatchReachesAnAcceptingConsumer()
    {
        var consumer = new CountingSignupConsumer();
        var dispatcher = new EventDispatcher([consumer]);

        await dispatcher.DispatchAsync(Recorded(), TestContext.Current.CancellationToken);

        Assert.Equal(Once, consumer.Calls);
    }

    [Fact]
    public async Task DispatchReachesEveryAcceptingConsumer()
    {
        var first = new CountingSignupConsumer();
        var second = new CountingSignupConsumer();
        var dispatcher = new EventDispatcher([first, second]);

        await dispatcher.DispatchAsync(Recorded(), TestContext.Current.CancellationToken);

        Assert.Equal(Once, first.Calls);
        Assert.Equal(Once, second.Calls);
    }

    [Fact]
    public async Task DispatchSkipsAConsumerOfAnotherEvent()
    {
        var consumer = new CountingSignupConsumer();
        var dispatcher = new EventDispatcher([consumer]);

        await dispatcher.DispatchAsync(
            new UnrelatedEvent(Guid.NewGuid(), DateTimeOffset.UnixEpoch),
            TestContext.Current.CancellationToken);

        Assert.Equal(Never, consumer.Calls);
    }

    [Fact]
    public async Task DispatchWithNoConsumersSucceeds()
    {
        var dispatcher = new EventDispatcher([]);

        await dispatcher.DispatchAsync(Recorded(), TestContext.Current.CancellationToken);
    }

    private static SignupRecorded Recorded() =>
        new(Guid.NewGuid(), DateTimeOffset.UnixEpoch, SignupId, Email, Plan, Seats);

    private sealed record UnrelatedEvent(Guid EventId, DateTimeOffset OccurredAt)
        : DomainEvent(EventId, OccurredAt);

    private sealed class CountingSignupConsumer : DomainEventConsumer<SignupRecorded>
    {
        public int Calls { get; private set; }

        protected override Task ConsumeAsync(SignupRecorded domainEvent, CancellationToken cancellationToken)
        {
            Calls++;
            return Task.CompletedTask;
        }
    }
}
