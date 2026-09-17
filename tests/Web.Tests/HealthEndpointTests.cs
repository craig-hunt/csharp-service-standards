using System.Net;
using System.Text.Json;
using ServiceStandards.Domain.Health;
using Xunit;

namespace ServiceStandards.Web.Tests;

/// <summary>
/// The probes a platform calls.
/// </summary>
/// <remarks>
/// Both routes answer without a token. A probe that needed a credential would
/// leave a platform unable to tell a stopped instance from an unauthorized one.
/// </remarks>
public sealed class HealthEndpointTests
{
    private const string StatusMember = "status";
    private const string ProbeFailure = "database unreachable";

    [Fact]
    public async Task LivenessAnswersWithoutACredential()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(
            new Uri(WebConstants.PathHealthLive, UriKind.Relative),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(HealthConstants.StatusOk, await ReadStatusAsync(response));
    }

    [Fact]
    public async Task ReadinessAnswersOkWhenTheProbeSucceeds()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(
            new Uri(WebConstants.PathHealthReady, UriKind.Relative),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(HealthConstants.StatusOk, await ReadStatusAsync(response));
    }

    [Fact]
    public async Task ReadinessReportsUnavailableWhenTheProbeFails()
    {
        using var factory = new ApiFactory();
        factory.Probe.Failure = new InvalidOperationException(ProbeFailure);
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(
            new Uri(WebConstants.PathHealthReady, UriKind.Relative),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal(HealthConstants.StatusUnavailable, await ReadStatusAsync(response));
    }

    private static async Task<string?> ReadStatusAsync(HttpResponseMessage response)
    {
        var payload = await response.Content
            .ReadAsStringAsync(TestContext.Current.CancellationToken)
            .ConfigureAwait(false);

        using var document = JsonDocument.Parse(payload);
        return document.RootElement.GetProperty(StatusMember).GetString();
    }
}
