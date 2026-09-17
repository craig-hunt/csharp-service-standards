using System.Collections.ObjectModel;

namespace ServiceStandards.Domain.Inventory;

/// <summary>
/// The rows a fresh database starts with, so the reference API answers with
/// something recognizable before anyone adds stock.
/// </summary>
public static class InventorySeed
{
    public static IReadOnlyList<InventoryItem> Items { get; } =
        new ReadOnlyCollection<InventoryItem>(
        [
            new InventoryItem(
                InventoryConstants.SeedAccessBadge,
                InventoryConstants.SeedAccessBadgeQuantity,
                Status.From(InventoryConstants.StatusInStock)),
            new InventoryItem(
                InventoryConstants.SeedDockingStation,
                InventoryConstants.SeedDockingStationQuantity,
                Status.From(InventoryConstants.StatusLow)),
            new InventoryItem(
                InventoryConstants.SeedLaptopSleeve,
                InventoryConstants.SeedLaptopSleeveQuantity,
                Status.From(InventoryConstants.StatusOutOfStock)),
            new InventoryItem(
                InventoryConstants.SeedMonitorArm,
                InventoryConstants.SeedMonitorArmQuantity,
                Status.From(InventoryConstants.StatusInStock)),
            new InventoryItem(
                InventoryConstants.SeedHeadset,
                InventoryConstants.SeedHeadsetQuantity,
                Status.From(InventoryConstants.StatusLow)),
            new InventoryItem(
                InventoryConstants.SeedWebcam,
                InventoryConstants.SeedWebcamQuantity,
                Status.From(InventoryConstants.StatusInStock)),
        ]);
}
