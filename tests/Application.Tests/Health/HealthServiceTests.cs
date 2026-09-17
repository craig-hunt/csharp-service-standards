using ServiceStandards.Application.Health;
using ServiceStandards.Application.Tests.Fakes;
using Xunit;

namespace ServiceStandards.Application.Tests.Health;

/// <summary>
/// Readiness reporting.
/// </summary>
/// <remarks>
/// The service reports a failure rather than logging it, which is what lets
/// these tests read the outcome directly instead of capturing log output.
/// </remarks>
public sealed class HealthServiceTests
{
    private const string FailureMessage = "database unreachable";

    [Fact]
    public async Task CheckAsyncReportsReadyWhenTheProbeAnswers()
    {
        var probe = new FakeHealthProbe();
        var service = new HealthService(probe);

        var check = await service.CheckAsync(TestContext.Current.CancellationToken);

        Assert.True(check.Ready);
        Assert.Null(check.Failure);
        Assert.True(probe.WasCalled);
    }

    [Fact]
    public async Task CheckAsyncReportsUnavailableWhenTheProbeFails()
    {
        var probe = new FakeHealthProbe { Failure = new InvalidOperationException(FailureMessage) };
        var service = new HealthService(probe);

        var check = await service.CheckAsync(TestContext.Current.CancellationToken);

        Assert.False(check.Ready);
        Assert.Equal(FailureMessage, check.Failure?.Message);
    }

    [Fact]
    public async Task CheckAsyncCarriesACancellationFailureThrough()
    {
        var probe = new FakeHealthProbe { Failure = new OperationCanceledException() };
        var service = new HealthService(probe);

        var check = await service.CheckAsync(TestContext.Current.CancellationToken);

        Assert.False(check.Ready);
        Assert.IsType<OperationCanceledException>(check.Failure);
    }

    [Fact]
    public async Task CheckAsyncRunsTheProbeUnderADeadline()
    {
        var probe = new FakeHealthProbe();
        var service = new HealthService(probe);

        await service.CheckAsync(TestContext.Current.CancellationToken);

        Assert.True(probe.SawCancellableToken);
    }
}
