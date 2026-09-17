using ServiceStandards.Domain.Inventory;

namespace ServiceStandards.Application.Abstractions;

/// <summary>
/// Reads the stock rows. Searching and ordering stay in the domain, so a second
/// store implementation cannot quietly answer with a different order.
/// </summary>
public interface IInventoryStore
{
    public Task<IReadOnlyList<InventoryItem>> ItemsAsync(CancellationToken cancellationToken);
}
