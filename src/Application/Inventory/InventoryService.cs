using ServiceStandards.Application.Abstractions;
using ServiceStandards.Domain.Inventory;

namespace ServiceStandards.Application.Inventory;

/// <summary>
/// Answers a stock query.
/// </summary>
public sealed class InventoryService(IInventoryStore store)
{
    public async Task<InventoryResult> QueryAsync(InventoryQuery query, CancellationToken cancellationToken)
    {
        var items = await store.ItemsAsync(cancellationToken).ConfigureAwait(false);
        return query.Apply(items);
    }
}
