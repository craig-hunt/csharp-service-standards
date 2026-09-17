using Microsoft.EntityFrameworkCore;
using ServiceStandards.Infrastructure.Persistence;
using Testcontainers.PostgreSql;
using Xunit;

namespace ServiceStandards.Integration.Tests;

/// <summary>
/// Starts one PostgreSQL container for the suite and applies the migrations to
/// it.
/// </summary>
/// <remarks>
/// The container starts and is removed by the run itself, so these tests never
/// touch a database anyone else owns and leave nothing behind. Running against
/// the real engine is the point: an in-memory provider accepts writes that a
/// check constraint rejects, and it orders rows by whatever the collation of the
/// moment happens to be.
/// </remarks>
public sealed class PostgresFixture : IAsyncLifetime
{
    private const string Image = "postgres:17-bookworm";
    private const string Database = "servicestandards";
    private const string User = "servicestandards";
    private const string Password = "servicestandards";

    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder(Image)
        .WithDatabase(Database)
        .WithUsername(User)
        .WithPassword(Password)
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync().ConfigureAwait(false);

        await using var database = CreateContext();
        await database.Database.MigrateAsync().ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync() => await _container.DisposeAsync().ConfigureAwait(false);

    public ServiceStandardsDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ServiceStandardsDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new ServiceStandardsDbContext(options);
    }
}
