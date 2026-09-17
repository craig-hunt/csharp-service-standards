using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using ServiceStandards.Application.Abstractions;
using ServiceStandards.Domain.Events;

namespace ServiceStandards.Infrastructure.Events;

/// <summary>
/// Records that a signup happened.
/// </summary>
/// <remarks>
/// A consumer lives with the module that reacts, not with the one that
/// published. This one sits here because the reference service has no second
/// module; a real consumer would be another service reading the same event off
/// a broker. Writing a log line twice harms nothing, which is the property
/// every consumer needs under at-least-once delivery.
/// </remarks>
[SuppressMessage(
    "Performance",
    "CA1812:Avoid uninstantiated internal classes",
    Justification = "The container activates this type as an IEventConsumer registration, so no code in this assembly constructs it.")]
internal sealed partial class SignupRecordedConsumer(ILogger<SignupRecordedConsumer> logger)
    : DomainEventConsumer<SignupRecorded>
{
    protected override Task ConsumeAsync(SignupRecorded domainEvent, CancellationToken cancellationToken)
    {
        LogSignupRecorded(logger, domainEvent.SignupId, domainEvent.Plan, domainEvent.Seats);
        return Task.CompletedTask;
    }

    [LoggerMessage(
        EventId = 10,
        Level = LogLevel.Information,
        Message = InfrastructureConstants.MsgSignupRecorded)]
    private static partial void LogSignupRecorded(ILogger logger, long signupId, string plan, int seats);
}
