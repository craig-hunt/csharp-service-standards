namespace ServiceStandards.Application.Abstractions;

/// <summary>
/// The current time, as a dependency rather than a static call.
/// </summary>
/// <remarks>
/// An event carries the moment it happened. Reading that from a static clock
/// leaves no way to assert the value a test expects.
/// </remarks>
public interface IClock
{
    public DateTimeOffset UtcNow { get; }
}
