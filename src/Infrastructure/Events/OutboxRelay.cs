using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ServiceStandards.Application.Abstractions;
using ServiceStandards.Infrastructure.Persistence;

namespace ServiceStandards.Infrastructure.Events;

/// <summary>
/// Publishes what the outbox holds, then records that it did.
/// </summary>
/// <remarks>
/// Publishing after the commit rather than during it is what makes delivery
/// at-least-once: the relay can deliver a message and fail before marking it,
/// and the next pass delivers it again. That trade is deliberate. The
/// alternative, marking first, loses messages instead, and a lost message is
/// worse than a repeated one for every consumer written to absorb repeats.
///
/// This reference runs the relay inside the API. A deployment with more than
/// one replica would run it as its own process, or claim rows with a row lock,
/// so two replicas never publish the same message.
/// </remarks>
[SuppressMessage(
    "Performance",
    "CA1812:Avoid uninstantiated internal classes",
    Justification = "The host activates this type through AddHostedService, so no code in this assembly constructs it.")]
internal sealed partial class OutboxRelay(IServiceScopeFactory scopes, ILogger<OutboxRelay> logger)
    : BackgroundService
{
    private const int Empty = 0;

    private static readonly TimeSpan PollInterval =
        TimeSpan.FromSeconds(InfrastructureConstants.OutboxPollSeconds);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await PublishPendingAsync(stoppingToken).ConfigureAwait(false);

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
        Justification = "A relay pass that fails must not end the background service; the next pass retries the same rows, which is exactly what at-least-once delivery relies on.")]
    private async Task PublishPendingAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = scopes.CreateScope();
            var database = scope.ServiceProvider.GetRequiredService<ServiceStandardsDbContext>();
            var dispatcher = scope.ServiceProvider.GetRequiredService<IEventDispatcher>();
            var clock = scope.ServiceProvider.GetRequiredService<IClock>();

            var pending = await database.OutboxMessages
                .Where(row => row.PublishedAt == null)
                .OrderBy(row => row.OccurredAt)
                .Take(InfrastructureConstants.OutboxBatchSize)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            if (pending.Count == Empty)
            {
                return;
            }

            foreach (var row in pending)
            {
                var message = OutboxSerializer.FromRow(row);
                if (message is not null)
                {
                    await dispatcher.DispatchAsync(message, cancellationToken).ConfigureAwait(false);
                }

                row.PublishedAt = clock.UtcNow;
            }

            await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception failure)
        {
            LogPublishFailed(logger, failure);
        }
    }
}
