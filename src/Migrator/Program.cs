using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Formatting.Json;
using ServiceStandards.Infrastructure;
using ServiceStandards.Infrastructure.Persistence;

namespace ServiceStandards.Migrator;

/// <summary>
/// The admin process that brings a database up to date.
/// </summary>
/// <remarks>
/// Migrations run here rather than at API startup. An API that migrates on boot
/// makes every replica race the others for the same lock, turns a rollback into
/// a deployment, and gives a platform no way to tell a failed migration from a
/// failed service. Compose and CI run this to completion first, and the exit
/// code says whether the API should start at all.
/// </remarks>
internal static class Program
{
    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "The entry point reports any failure as a nonzero exit code after logging it; a narrower catch would lose the log line and the flush for unanticipated failure types.")]
    public static async Task<int> Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(new JsonFormatter())
            .CreateLogger();

        try
        {
            var builder = Host.CreateApplicationBuilder(args);
            var connectionString =
                builder.Configuration.GetConnectionString(InfrastructureConstants.ConnectionName)
                ?? throw new InvalidOperationException(MigratorConstants.MsgMissingConnection);

            builder.Services.AddInfrastructure(connectionString);

            using var host = builder.Build();
            using var scope = host.Services.CreateScope();
            var database = scope.ServiceProvider.GetRequiredService<ServiceStandardsDbContext>();

            Log.Information(MigratorConstants.MsgApplyingMigrations);
            await database.Database.MigrateAsync().ConfigureAwait(false);

            Log.Information(MigratorConstants.MsgSeeding);
            await DatabaseSeeder.SeedAsync(database, CancellationToken.None).ConfigureAwait(false);

            Log.Information(MigratorConstants.MsgComplete);
            return MigratorConstants.ExitSuccess;
        }
        catch (Exception failure)
        {
            Log.Fatal(failure, MigratorConstants.MsgFailed);
            return MigratorConstants.ExitFailure;
        }
        finally
        {
            await Log.CloseAndFlushAsync().ConfigureAwait(false);
        }
    }
}
