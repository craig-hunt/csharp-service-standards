namespace ServiceStandards.Application.Abstractions;

/// <summary>
/// Confirms that the backing database answers.
/// </summary>
public interface IHealthProbe
{
    public Task PingAsync(CancellationToken cancellationToken);
}
