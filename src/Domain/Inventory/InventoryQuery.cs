using System.Collections.ObjectModel;
using ServiceStandards.Domain.Errors;

namespace ServiceStandards.Domain.Inventory;

/// <summary>
/// A validated stock query: what to search for, which column orders the rows,
/// and which way that order runs.
/// </summary>
/// <remarks>
/// The sibling Go project parses this straight from the query string. Here the
/// factory takes three values instead, so the domain states the rule without
/// knowing that a query string exists or what its keys are called.
/// </remarks>
public sealed record InventoryQuery(string Search, Column Sort, Direction Direction)
{
    public static InventoryQuery From(string? search, string? sort, string? direction)
    {
        var column = Column.From(sort);
        if (column.IsAbsent)
        {
            column = Column.From(InventoryConstants.ColumnName);
        }
        else if (!column.Known)
        {
            throw new InvalidSortException();
        }

        var order = Direction.From(direction);
        if (order.IsAbsent)
        {
            order = Direction.From(InventoryConstants.DirectionAscending);
        }
        else if (!order.Known)
        {
            throw new InvalidDirectionException();
        }

        return new InventoryQuery((search ?? string.Empty).Trim(), column, order);
    }

    /// <summary>
    /// Filters and orders a set of rows, leaving the caller's list untouched.
    /// </summary>
    /// <remarks>
    /// The order is stable, so rows that tie on the sort column keep the order
    /// the store returned rather than shuffling between requests.
    /// </remarks>
    public InventoryResult Apply(IReadOnlyList<InventoryItem> items)
    {
        var matched = items
            .Where(item => item.Name.Contains(Search, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var ordered = Sort.Value switch
        {
            InventoryConstants.ColumnQuantity => Direction.Descending
                ? matched.OrderByDescending(item => item.Quantity)
                : matched.OrderBy(item => item.Quantity),
            InventoryConstants.ColumnStatus => Direction.Descending
                ? matched.OrderByDescending(item => item.Status.Value, StringComparer.Ordinal)
                : matched.OrderBy(item => item.Status.Value, StringComparer.Ordinal),
            _ => Direction.Descending
                ? matched.OrderByDescending(item => item.Name, StringComparer.Ordinal)
                : matched.OrderBy(item => item.Name, StringComparer.Ordinal),
        };

        var shown = new ReadOnlyCollection<InventoryItem>(ordered.ToList());
        return new InventoryResult(shown, shown.Count, items.Count);
    }
}
