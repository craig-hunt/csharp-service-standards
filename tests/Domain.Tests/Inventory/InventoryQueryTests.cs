using ServiceStandards.Domain.Errors;
using ServiceStandards.Domain.Inventory;
using Xunit;

namespace ServiceStandards.Domain.Tests.Inventory;

/// <summary>
/// Stock searching and ordering.
/// </summary>
/// <remarks>
/// The stability case uses two rows that tie on the sort column and asserts the
/// order the caller supplied survives. An unstable sort passes every other test
/// here.
/// </remarks>
public sealed class InventoryQueryTests
{
    private const string Alpha = "Alpha cable";
    private const string Bravo = "Bravo cable";
    private const string Charlie = "Charlie dock";
    private const string SearchCable = "cable";
    private const string SearchUpper = "CABLE";
    private const string SearchMissing = "nothing here";
    private const string UnknownColumn = "colour";
    private const string UnknownDirection = "sideways";
    private const int TiedQuantity = 5;
    private const int HigherQuantity = 9;
    private const int TotalRows = 3;
    private const int MatchingRows = 2;

    [Fact]
    public void FromDefaultsToNameAscending()
    {
        var query = InventoryQuery.From(null, null, null);

        Assert.Equal(InventoryConstants.ColumnName, query.Sort.Value);
        Assert.Equal(InventoryConstants.DirectionAscending, query.Direction.Value);
    }

    [Fact]
    public void FromRejectsAnUnknownColumn() =>
        Assert.Throws<InvalidSortException>(() => InventoryQuery.From(null, UnknownColumn, null));

    [Fact]
    public void FromRejectsAnUnknownDirection() =>
        Assert.Throws<InvalidDirectionException>(() => InventoryQuery.From(null, null, UnknownDirection));

    [Theory]
    [InlineData("name")]
    [InlineData("quantity")]
    [InlineData("status")]
    public void FromAcceptsEveryKnownColumn(string column) =>
        Assert.Equal(column, InventoryQuery.From(null, column, null).Sort.Value);

    [Fact]
    public void ApplyOrdersByNameAscendingByDefault()
    {
        var result = InventoryQuery.From(null, null, null).Apply(Sample());

        Assert.Equal(Alpha, result.Items[0].Name);
        Assert.Equal(Charlie, result.Items[^1].Name);
    }

    [Fact]
    public void ApplyReversesOrderWhenDescending()
    {
        var query = InventoryQuery.From(null, null, InventoryConstants.DirectionDescending);

        var result = query.Apply(Sample());

        Assert.Equal(Charlie, result.Items[0].Name);
        Assert.Equal(Alpha, result.Items[^1].Name);
    }

    [Fact]
    public void ApplySortsTiedRowsStably()
    {
        var query = InventoryQuery.From(null, InventoryConstants.ColumnQuantity, null);

        var result = query.Apply(Sample());

        Assert.Equal(Bravo, result.Items[0].Name);
        Assert.Equal(Alpha, result.Items[1].Name);
    }

    [Fact]
    public void ApplyMatchesNamesWithoutRegardToCase()
    {
        var result = InventoryQuery.From(SearchUpper, null, null).Apply(Sample());

        Assert.Equal(MatchingRows, result.Shown);
    }

    [Fact]
    public void ApplyReportsTotalAcrossEveryRow()
    {
        var result = InventoryQuery.From(SearchCable, null, null).Apply(Sample());

        Assert.Equal(MatchingRows, result.Shown);
        Assert.Equal(TotalRows, result.Total);
    }

    [Fact]
    public void ApplyReportsNothingWhenNoNameMatches()
    {
        var result = InventoryQuery.From(SearchMissing, null, null).Apply(Sample());

        Assert.Empty(result.Items);
        Assert.Equal(TotalRows, result.Total);
    }

    [Fact]
    public void ApplyLeavesTheCallersListUntouched()
    {
        var items = Sample();

        InventoryQuery.From(null, null, InventoryConstants.DirectionDescending).Apply(items);

        Assert.Equal(Bravo, items[0].Name);
    }

    private static IReadOnlyList<InventoryItem> Sample() =>
    [
        new InventoryItem(Bravo, TiedQuantity, Status.From(InventoryConstants.StatusLow)),
        new InventoryItem(Alpha, TiedQuantity, Status.From(InventoryConstants.StatusInStock)),
        new InventoryItem(Charlie, HigherQuantity, Status.From(InventoryConstants.StatusOutOfStock)),
    ];
}
