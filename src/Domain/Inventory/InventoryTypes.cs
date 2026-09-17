namespace ServiceStandards.Domain.Inventory;

/// <summary>
/// A column the stock table sorts on.
/// </summary>
public readonly record struct Column
{
    private Column(string value) => Value = value;

    public string Value { get; }

    public bool IsAbsent => string.IsNullOrEmpty(Value);

    public bool Known =>
        Value == InventoryConstants.ColumnName
        || Value == InventoryConstants.ColumnQuantity
        || Value == InventoryConstants.ColumnStatus;

    public static Column From(string? raw) => new(raw ?? string.Empty);

    public override string ToString() => Value;
}

/// <summary>
/// Which way a sort runs.
/// </summary>
public readonly record struct Direction
{
    private Direction(string value) => Value = value;

    public string Value { get; }

    public bool IsAbsent => string.IsNullOrEmpty(Value);

    public bool Known =>
        Value == InventoryConstants.DirectionAscending
        || Value == InventoryConstants.DirectionDescending;

    public bool Descending => Value == InventoryConstants.DirectionDescending;

    public static Direction From(string? raw) => new(raw ?? string.Empty);

    public override string ToString() => Value;
}

/// <summary>
/// The stock status a row reports, in the words the table shows.
/// </summary>
public readonly record struct Status
{
    private Status(string value) => Value = value;

    public string Value { get; }

    public static Status From(string? raw) => new(raw ?? string.Empty);

    public override string ToString() => Value;
}

/// <summary>
/// One row of the stock table.
/// </summary>
public sealed record InventoryItem(string Name, int Quantity, Status Status);

/// <summary>
/// What a stock query answers: the rows that matched, how many matched, and how
/// many rows exist.
/// </summary>
public sealed record InventoryResult(IReadOnlyList<InventoryItem> Items, int Shown, int Total);
