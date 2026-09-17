namespace ServiceStandards.Migrator;

/// <summary>
/// Every literal the migrator carries, named once.
/// </summary>
internal static class MigratorConstants
{
    public const int ExitSuccess = 0;
    public const int ExitFailure = 1;

    public const string MsgMissingConnection =
        "the migrator needs a connection string named Default";
    public const string MsgApplyingMigrations = "applying migrations";
    public const string MsgSeeding = "seeding demo data";
    public const string MsgComplete = "database is up to date";
    public const string MsgFailed = "the migrator could not finish";
}
