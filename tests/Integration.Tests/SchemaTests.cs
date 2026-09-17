using Microsoft.EntityFrameworkCore;
using ServiceStandards.Domain.Inventory;
using ServiceStandards.Domain.Signups;
using ServiceStandards.Infrastructure.Persistence;
using ServiceStandards.Infrastructure.Stores;
using Xunit;

namespace ServiceStandards.Integration.Tests;

/// <summary>
/// What the database enforces on its own.
/// </summary>
/// <remarks>
/// The check constraints exist so that a defect in the application cannot write
/// a row the domain would reject. Asserting them here proves the constraint
/// reached the schema, which a test against the application alone cannot show.
/// </remarks>
public sealed class SchemaTests(PostgresFixture fixture)
    : IClassFixture<PostgresFixture>
{
    private const string Name = "Schema Tester";
    private const string Email = "schema@example.com";
    private const string Notes = "Checking the constraints.";
    private const int ValidSeats = 3;
    private const int InvalidSeats = 0;
    private const string UnknownPlan = "Platinum";
    private const string ItemName = "Constraint widget";
    private const int NegativeQuantity = -1;
    private const int ValidQuantity = 4;
    private const string UnknownStatus = "Exploded";
    private const long NoIdentifier = 0;

    [Fact]
    public async Task SignupSaveAssignsAGeneratedIdentifier()
    {
        await using var database = fixture.CreateContext();
        var store = new EfSignupStore(database, new FixedClock());

        var id = await store.SaveAsync(
            new Signup(Name, Email, Plan.From(SignupConstants.PlanGrowth), ValidSeats, Notes),
            TestContext.Current.CancellationToken);

        Assert.True(id.Value > NoIdentifier);
    }

    [Fact]
    public async Task TheDatabaseRejectsAnUnknownPlan()
    {
        await using var database = fixture.CreateContext();
        database.Signups.Add(new SignupRow
        {
            FullName = Name,
            Email = Email,
            Plan = UnknownPlan,
            Seats = ValidSeats,
            Notes = Notes,
        });

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            database.SaveChangesAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TheDatabaseRejectsASeatCountBelowOne()
    {
        await using var database = fixture.CreateContext();
        database.Signups.Add(new SignupRow
        {
            FullName = Name,
            Email = Email,
            Plan = SignupConstants.PlanStarter,
            Seats = InvalidSeats,
            Notes = Notes,
        });

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            database.SaveChangesAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TheDatabaseRejectsANegativeQuantity()
    {
        await using var database = fixture.CreateContext();
        database.InventoryItems.Add(new InventoryRow
        {
            Name = ItemName,
            Quantity = NegativeQuantity,
            Status = InventoryConstants.StatusInStock,
        });

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            database.SaveChangesAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TheDatabaseRejectsAnUnknownStatus()
    {
        await using var database = fixture.CreateContext();
        database.InventoryItems.Add(new InventoryRow
        {
            Name = ItemName,
            Quantity = ValidQuantity,
            Status = UnknownStatus,
        });

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            database.SaveChangesAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task SeedingTwiceChangesNothing()
    {
        await using var first = fixture.CreateContext();
        await DatabaseSeeder.SeedAsync(first, TestContext.Current.CancellationToken);

        await using var second = fixture.CreateContext();
        await DatabaseSeeder.SeedAsync(second, TestContext.Current.CancellationToken);

        await using var reader = fixture.CreateContext();
        var stocked = await reader.InventoryItems
            .AsNoTracking()
            .CountAsync(TestContext.Current.CancellationToken);

        Assert.Equal(InventorySeed.Items.Count, stocked);
    }
}
