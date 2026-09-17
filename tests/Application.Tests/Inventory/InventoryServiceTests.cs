using ServiceStandards.Application.Inventory;
using ServiceStandards.Application.Tests.Fakes;
using ServiceStandards.Domain.Inventory;
using Xunit;

namespace ServiceStandards.Application.Tests.Inventory;

/// <summary>
/// The stock service reads, then lets the domain search and order.
/// </summary>
public sealed class InventoryServiceTests
{
    private const string Alpha = "Alpha cable";
    private const string Bravo = "Bravo dock";
    private const string Search = "alpha";
    private const int AlphaQuantity = 3;
    private const int BravoQuantity = 8;
    private const int TotalRows = 2;

    [Fact]
    public async Task QueryAsyncOrdersByNameByDefault()
    {
        var service = new InventoryService(SeededStore());

        var result = await service.QueryAsync(
            InventoryQuery.From(null, null, null),
            TestContext.Current.CancellationToken);

        Assert.Equal(Alpha, result.Items[0].Name);
        Assert.Equal(TotalRows, result.Total);
    }

    [Fact]
    public async Task QueryAsyncAppliesTheSearch()
    {
        var service = new InventoryService(SeededStore());

        var result = await service.QueryAsync(
            InventoryQuery.From(Search, null, null),
            TestContext.Current.CancellationToken);

        var shown = Assert.Single(result.Items);
        Assert.Equal(Alpha, shown.Name);
        Assert.Equal(TotalRows, result.Total);
    }

    [Fact]
    public async Task QueryAsyncAppliesTheDirection()
    {
        var service = new InventoryService(SeededStore());

        var result = await service.QueryAsync(
            InventoryQuery.From(null, null, InventoryConstants.DirectionDescending),
            TestContext.Current.CancellationToken);

        Assert.Equal(Bravo, result.Items[0].Name);
    }

    [Fact]
    public async Task QueryAsyncReportsNothingForAnEmptyStore()
    {
        var service = new InventoryService(new FakeInventoryStore());

        var result = await service.QueryAsync(
            InventoryQuery.From(null, null, null),
            TestContext.Current.CancellationToken);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.Total);
    }

    private static FakeInventoryStore SeededStore()
    {
        var store = new FakeInventoryStore();
        store.Seed(
            new InventoryItem(Bravo, BravoQuantity, Status.From(InventoryConstants.StatusInStock)),
            new InventoryItem(Alpha, AlphaQuantity, Status.From(InventoryConstants.StatusLow)));
        return store;
    }
}
