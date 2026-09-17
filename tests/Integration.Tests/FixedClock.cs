using System.Globalization;
using ServiceStandards.Application.Abstractions;

namespace ServiceStandards.Integration.Tests;

/// <summary>
/// A clock that always reports the same moment.
/// </summary>
/// <remarks>
/// An event records when it happened, and asserting that value needs a time the
/// test chose rather than the one the machine held when it ran. Shared across
/// the suites so every store built here reads the same instant.
/// </remarks>
internal sealed class FixedClock : IClock
{
    private const string MomentText = "2026-09-17T12:00:00+00:00";

    public static DateTimeOffset Moment { get; } =
        DateTimeOffset.Parse(MomentText, CultureInfo.InvariantCulture);

    public DateTimeOffset UtcNow => Moment;
}
