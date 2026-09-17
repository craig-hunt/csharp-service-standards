using Microsoft.EntityFrameworkCore;
using ServiceStandards.Application.Abstractions;
using ServiceStandards.Infrastructure.Persistence;

namespace ServiceStandards.Infrastructure.Health;

/// <summary>
/// Answers a readiness probe by asking the database to run a trivial statement.
/// </summary>
/// <remarks>
/// Opening a connection proves less than running a statement on it: a pool can
/// hand back a connection to a database that has since stopped accepting
/// queries.
/// </remarks>
public sealed class DatabaseProbe(ServiceStandardsDbContext database)
    : IHealthProbe
{
    public async Task PingAsync(CancellationToken cancellationToken) =>
        await database.Database
            .ExecuteSqlRawAsync(InfrastructureConstants.PingSql, cancellationToken)
            .ConfigureAwait(false);
}
