using Microsoft.EntityFrameworkCore;
using ServiceStandards.Domain.Inventory;
using ServiceStandards.Domain.Tasks;

namespace ServiceStandards.Infrastructure.Persistence;

/// <summary>
/// Loads the demo data the migrator installs.
/// </summary>
/// <remarks>
/// Running it twice changes nothing: tasks load only into an empty table, and
/// stock rows update in place by name. An admin process that cannot run twice
/// safely is one nobody can run confidently.
/// </remarks>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(ServiceStandardsDbContext database, CancellationToken cancellationToken)
    {
        var anyTasks = await database.Tasks.AnyAsync(cancellationToken).ConfigureAwait(false);
        if (!anyTasks)
        {
            database.Tasks.Add(new TaskRow { Title = TaskConstants.SeedTitleFirst });
            database.Tasks.Add(new TaskRow { Title = TaskConstants.SeedTitleSecond });
        }

        foreach (var item in InventorySeed.Items)
        {
            var existing = await database.InventoryItems
                .FirstOrDefaultAsync(row => row.Name == item.Name, cancellationToken)
                .ConfigureAwait(false);

            if (existing is null)
            {
                database.InventoryItems.Add(new InventoryRow
                {
                    Name = item.Name,
                    Quantity = item.Quantity,
                    Status = item.Status.Value,
                });
            }
            else
            {
                existing.Quantity = item.Quantity;
                existing.Status = item.Status.Value;
            }
        }

        await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
