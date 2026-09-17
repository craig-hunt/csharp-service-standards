using Microsoft.EntityFrameworkCore;
using ServiceStandards.Application.Abstractions;
using ServiceStandards.Domain.Inventory;
using ServiceStandards.Infrastructure.Persistence;

namespace ServiceStandards.Infrastructure.Stores;

/// <summary>
/// Reads stock rows through EF Core.
/// </summary>
/// <remarks>
/// The store returns rows in name order and applies no search or sort of its
/// own. Those rules live in the domain, so every store answers identically.
/// </remarks>
public sealed class EfInventoryStore(ServiceStandardsDbContext database)
    : IInventoryStore
{
    public async Task<IReadOnlyList<InventoryItem>> ItemsAsync(CancellationToken cancellationToken)
    {
        var rows = await database.InventoryItems
            .AsNoTracking()
            .OrderBy(row => row.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows.ConvertAll(Map);
    }

    private static InventoryItem Map(InventoryRow row) =>
        new(row.Name, row.Quantity, Status.From(row.Status));
}
