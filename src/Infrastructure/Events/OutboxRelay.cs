using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ServiceStandards.Infrastructure.Events;

/// <summary>
/// Wakes on a timer and asks the publisher to drain the outbox.
/// </summary>
/// <remarks>
/// Scheduling only. The delivery rules live in OutboxPublisher, which a test
/// drives directly rather than through a background service and a clock.
/// </remarks>
[SuppressMessage(
    "Performance",
    "CA1812:Avoid uninstantiated internal classes",
    Justification = "The host activates this type through AddHostedService, so no code in this assembly constructs it.")]
internal sealed partial class OutboxRelay(IServiceScopeFactory scopes, ILogger<OutboxRelay> logger)
    : BackgroundService
{
    private static readonly TimeSpan PollInterval =
        TimeSpan.FromSeconds(InfrastructureConstants.OutboxPollSeconds);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await PublishOnceAsync(stoppingToken).ConfigureAwait(false);

            try
            {
                await Task.Delay(PollInterval, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }

    [LoggerMessage(
        EventId = 11,
        Level = LogLevel.Error,
        Message = InfrastructureConstants.MsgOutboxPublishFailed)]
    private static partial void LogPublishFailed(ILogger logger, Exception failure);

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "A pass that fails must not end the background service; the next pass retries the same rows, which is what at-least-once delivery relies on.")]
    private async Task PublishOnceAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = scopes.CreateScope();
            var publisher = scope.ServiceProvider.GetRequiredService<OutboxPublisher>();
            await publisher.PublishPendingAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception failure)
        {
            LogPublishFailed(logger, failure);
        }
    }
}
