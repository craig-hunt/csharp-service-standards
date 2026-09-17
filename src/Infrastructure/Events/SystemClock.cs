using System.Diagnostics.CodeAnalysis;
using ServiceStandards.Application.Abstractions;

namespace ServiceStandards.Infrastructure.Events;

/// <summary>The machine clock, in UTC.</summary>
[SuppressMessage(
    "Performance",
    "CA1812:Avoid uninstantiated internal classes",
    Justification = "The container activates this type as the IClock registration, so no code in this assembly constructs it.")]
internal sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
