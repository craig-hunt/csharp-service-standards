namespace ServiceStandards.Domain.Inventory;

/// <summary>
/// Every literal the inventory feature carries, named once.
/// </summary>
public static class InventoryConstants
{
    public const string ColumnName = "name";
    public const string ColumnQuantity = "quantity";
    public const string ColumnStatus = "status";

    public const string DirectionAscending = "ascending";
    public const string DirectionDescending = "descending";

    public const string StatusInStock = "In stock";
    public const string StatusLow = "Low";
    public const string StatusOutOfStock = "Out of stock";

    public const string CodeInvalidQuery = "invalid_query";
    public const string MsgInvalidSort = "sort must be name, quantity, or status";
    public const string MsgInvalidDirection = "direction must be ascending or descending";

    public const string SeedAccessBadge = "Access badge";
    public const string SeedDockingStation = "Docking station";
    public const string SeedLaptopSleeve = "Laptop sleeve";
    public const string SeedMonitorArm = "Monitor arm";
    public const string SeedHeadset = "Noise-cancelling headset";
    public const string SeedWebcam = "Webcam";

    public const int SeedAccessBadgeQuantity = 240;
    public const int SeedDockingStationQuantity = 12;
    public const int SeedLaptopSleeveQuantity = 0;
    public const int SeedMonitorArmQuantity = 58;
    public const int SeedHeadsetQuantity = 4;
    public const int SeedWebcamQuantity = 31;
}
