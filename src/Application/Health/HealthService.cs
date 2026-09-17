using System.Diagnostics.CodeAnalysis;
using ServiceStandards.Application.Abstractions;

namespace ServiceStandards.Application.Health;

/// <summary>
/// The outcome of a readiness probe: whether the dependency answered, and the
/// failure to log when it did not.
/// </summary>
public sealed record ReadinessCheck(bool Ready, Exception? Failure);

/// <summary>
/// Runs the readiness probe under a deadline.
/// </summary>
/// <remarks>
/// The service reports the failure rather than logging it, so the logger stays
/// a concern of the layer that owns request context, and a test reads the
/// outcome without capturing log output.
/// </remarks>
public sealed class HealthService(IHealthProbe probe)
{
    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "Readiness reports unavailable for any failure the dependency raises; narrowing the catch would let an unanticipated failure type escape and return a 500 in place of an unavailable status.")]
    public async Task<ReadinessCheck> CheckAsync(CancellationToken cancellationToken)
    {
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(Domain.Health.HealthConstants.ReadyTimeout);

        try
        {
            await probe.PingAsync(deadline.Token).ConfigureAwait(false);
            return new ReadinessCheck(true, null);
        }
        catch (Exception failure)
        {
            return new ReadinessCheck(false, failure);
        }
    }
}
