using ServiceStandards.Application.Abstractions;
using ServiceStandards.Domain.Signups;
using ServiceStandards.Infrastructure.Events;
using ServiceStandards.Infrastructure.Persistence;

namespace ServiceStandards.Infrastructure.Stores;

/// <summary>
/// Records signups through EF Core, and the event that says one happened.
/// </summary>
/// <remarks>
/// The signup row and the outbox row commit together. Two saves run inside one
/// transaction because the database assigns the identifier on the first, and
/// the event carries it. Either both rows land or neither does, so no consumer
/// ever hears about a signup that failed to store, and no stored signup goes
/// unannounced.
/// </remarks>
public sealed class EfSignupStore(ServiceStandardsDbContext database, IClock clock)
    : ISignupStore
{
    public async Task<SignupId> SaveAsync(Signup signup, CancellationToken cancellationToken)
    {
        var transaction = await database.Database
            .BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);

        await using (transaction.ConfigureAwait(false))
        {
            var row = new SignupRow
            {
                FullName = signup.FullName,
                Email = signup.Email,
                Plan = signup.Plan.Value,
                Seats = signup.Seats,
                Notes = signup.Notes,
            };

            database.Signups.Add(row);
            await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            var recorded = signup.Recorded(row.Id, Guid.NewGuid(), clock.UtcNow);
            database.OutboxMessages.Add(OutboxSerializer.ToRow(recorded));
            await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return row.Id;
        }
    }
}
