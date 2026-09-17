namespace ServiceStandards.Domain.Health;

/// <summary>
/// Every literal the health feature carries, named once.
/// </summary>
public static class HealthConstants
{
    public const string StatusOk = "ok";
    public const string StatusUnavailable = "unavailable";
    public const string MsgNotReady = "readiness check failed";

    private const int ReadyTimeoutSeconds = 2;

    /// <summary>
    /// Gets how long a readiness probe waits on the database before reporting
    /// the instance unavailable.
    /// </summary>
    /// <remarks>
    /// A probe that waits as long as the database takes turns a slow dependency
    /// into a hung probe, and the platform then keeps routing traffic to an
    /// instance that cannot serve it.
    /// </remarks>
    public static TimeSpan ReadyTimeout { get; } = TimeSpan.FromSeconds(ReadyTimeoutSeconds);
}

/// <summary>
/// What a health probe answers.
/// </summary>
public sealed record HealthReport(string Status);
