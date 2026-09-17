using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ServiceStandards.Application.Abstractions;
using ServiceStandards.Infrastructure.Persistence;

namespace ServiceStandards.Infrastructure.Events;

/// <summary>
/// Claims a batch of outbox messages, delivers them, and marks what it
/// delivered.
/// </summary>
/// <remarks>
/// The claim runs FOR UPDATE SKIP LOCKED inside a transaction, so a second
/// replica running the same relay takes a different batch rather than the same
/// rows. Without that, two relays read identical rows and every consumer sees
/// each message twice. At-least-once delivery tolerates a repeat; it is no
/// reason to manufacture one on every pass.
///
/// A message whose type this service cannot resolve stays unpublished.
/// Marking it would acknowledge something no consumer ever saw, which is the
/// one outcome the outbox exists to prevent. It waits for a deployment that
/// knows the type, and a production system would move it aside once a retry
/// budget ran out.
///
/// Separate from the relay so the delivery path has a seam a test can drive
/// without a background service and a timer.
/// </remarks>
public sealed partial class OutboxPublisher(
    ServiceStandardsDbContext database,
    IEventDispatcher dispatcher,
    IClock clock,
    ILogger<OutboxPublisher> logger)
{
    private const int Empty = 0;

    /// <summary>Delivers one batch and answers how many it published.</summary>
    public async Task<int> PublishPendingAsync(CancellationToken cancellationToken)
    {
        var transaction = await database.Database
            .BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);

        await using (transaction.ConfigureAwait(false))
        {
            var claimed = await database.OutboxMessages
                .FromSqlRaw(InfrastructureConstants.SqlClaimOutbox, InfrastructureConstants.OutboxBatchSize)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            var published = Empty;

            foreach (var row in claimed)
            {
                var message = OutboxSerializer.FromRow(row);
                if (message is null)
                {
                    LogUnknownType(logger, row.EventId, row.Type);
                    continue;
                }

                await dispatcher.DispatchAsync(message, cancellationToken).ConfigureAwait(false);
                row.PublishedAt = clock.UtcNow;
                published++;
            }

            if (published > Empty)
            {
                await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return published;
        }
    }

    [LoggerMessage(
        EventId = 12,
        Level = LogLevel.Error,
        Message = InfrastructureConstants.MsgOutboxUnknownType)]
    private static partial void LogUnknownType(ILogger logger, Guid eventId, string type);
}
