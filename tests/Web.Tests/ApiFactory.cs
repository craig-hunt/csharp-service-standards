using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ServiceStandards.Application.Abstractions;
using ServiceStandards.Domain.Inventory;
using ServiceStandards.Domain.Signups;
using ServiceStandards.Domain.Tasks;

namespace ServiceStandards.Web.Tests;

/// <summary>
/// Runs the real application with fake stores behind it.
/// </summary>
/// <remarks>
/// The composition root, routing, authentication, serialization, and the error
/// middleware are all the production ones, so these tests exercise what a client
/// actually meets. Only the stores are replaced, which keeps the suite fast and
/// keeps its failures pointing at the HTTP surface rather than at a database.
/// </remarks>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    public const string Token = "test-token";

    private const string Disabled = "false";
    private const string SigningKey = "a-signing-key-long-enough-for-hmac-sha256";
    private const string UnusedConnection = "Host=unused;Database=unused;Username=unused;Password=unused";
    private const string ConnectionSetting = "ConnectionStrings:Default";

    public FakeTaskStore Tasks { get; } = new();

    public FakeSignupStore Signups { get; } = new();

    public FakeInventoryStore Inventory { get; } = new();

    public FakeHealthProbe Probe { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting(ConnectionSetting, UnusedConnection);
        builder.UseSetting(WebConstants.ConfigApiToken, Token);
        builder.UseSetting(WebConstants.ConfigJwtSigningKey, SigningKey);
        builder.UseSetting(WebConstants.ConfigOutboxEnabled, Disabled);

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ITaskStore>();
            services.RemoveAll<ISignupStore>();
            services.RemoveAll<IInventoryStore>();
            services.RemoveAll<IHealthProbe>();

            services.AddSingleton<ITaskStore>(Tasks);
            services.AddSingleton<ISignupStore>(Signups);
            services.AddSingleton<IInventoryStore>(Inventory);
            services.AddSingleton<IHealthProbe>(Probe);
        });
    }
}

/// <summary>A task store that serves whatever it was seeded.</summary>
public sealed class FakeTaskStore : ITaskStore
{
    private const long FirstId = 1;
    private const string StoredTitle = "Stored task";

    private readonly List<TaskItem> _items = [];

    private long _nextId = FirstId;

    public bool NotFound { get; set; }

    public long ClearResult { get; set; }

    /// <summary>
    /// Returns the store to its starting state.
    /// </summary>
    /// <remarks>
    /// The factory is shared across the class so the host starts once, which
    /// means this store is shared too. Without a reset between tests, one test's
    /// writes become the next test's starting data and the suite starts passing
    /// or failing by execution order.
    /// </remarks>
    public void Reset()
    {
        _items.Clear();
        _nextId = FirstId;
        NotFound = false;
        ClearResult = 0;
    }

    public void Seed(params TaskItem[] items) => _items.AddRange(items);

    public Task<IReadOnlyList<TaskItem>> ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<TaskItem>>(_items);

    public Task<TaskItem> CreateAsync(TaskTitle title, CancellationToken cancellationToken)
    {
        var created = new TaskItem(TaskId.From(_nextId), title, false);
        _nextId++;
        _items.Add(created);
        return Task.FromResult(created);
    }

    public Task<TaskItem> SetCompletedAsync(TaskId id, bool completed, CancellationToken cancellationToken) =>
        NotFound
            ? throw new Domain.Errors.TaskNotFoundException()
            : Task.FromResult(new TaskItem(id, TaskTitle.From(StoredTitle), completed));

    public Task DeleteAsync(TaskId id, CancellationToken cancellationToken) =>
        NotFound ? throw new Domain.Errors.TaskNotFoundException() : Task.CompletedTask;

    public Task<long> DeleteCompletedAsync(CancellationToken cancellationToken) =>
        Task.FromResult(ClearResult);
}

/// <summary>A signup store that answers with a fixed identifier.</summary>
public sealed class FakeSignupStore : ISignupStore
{
    private const long AssignedId = 42;

    public Task<SignupId> SaveAsync(Signup signup, CancellationToken cancellationToken) =>
        Task.FromResult(SignupId.From(AssignedId));
}

/// <summary>A stock store that serves whatever it was seeded.</summary>
public sealed class FakeInventoryStore : IInventoryStore
{
    private readonly List<InventoryItem> _items = [];

    public void Seed(params InventoryItem[] items) => _items.AddRange(items);

    public Task<IReadOnlyList<InventoryItem>> ItemsAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<InventoryItem>>(_items);
}

/// <summary>A probe that answers or fails on demand.</summary>
public sealed class FakeHealthProbe : IHealthProbe
{
    public Exception? Failure { get; set; }

    public Task PingAsync(CancellationToken cancellationToken) =>
        Failure is null ? Task.CompletedTask : Task.FromException(Failure);
}
