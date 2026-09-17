using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ServiceStandards.Infrastructure.Persistence;

namespace ServiceStandards.Migrator;

/// <summary>
/// Builds a context for the EF tooling, which runs without configuration.
/// </summary>
/// <remarks>
/// Writing a migration needs the provider and the model, never a reachable
/// database, so the tooling gets a placeholder connection string. Without this
/// the tooling runs the entry point instead, which refuses to start without a
/// real connection string, and no migration can be written at all.
///
/// The placeholder never reaches a deployed environment: the running migrator
/// reads its connection string from configuration and fails loudly when it is
/// missing.
/// </remarks>
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Performance",
    "CA1812:Avoid uninstantiated internal classes",
    Justification = "The EF tooling discovers and activates this type by reflection at design time, so no code in this assembly constructs it.")]
internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ServiceStandardsDbContext>
{
    private const string DesignTimeConnection =
        "Host=localhost;Database=servicestandards;Username=servicestandards;Password=servicestandards";

    public ServiceStandardsDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ServiceStandardsDbContext>()
            .UseNpgsql(DesignTimeConnection)
            .Options;

        return new ServiceStandardsDbContext(options);
    }
}
