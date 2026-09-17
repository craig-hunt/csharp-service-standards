using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServiceStandards.Application.Abstractions;
using ServiceStandards.Application.Events;
using ServiceStandards.Infrastructure.Events;
using ServiceStandards.Infrastructure.Health;
using ServiceStandards.Infrastructure.Persistence;
using ServiceStandards.Infrastructure.Stores;

namespace ServiceStandards.Infrastructure;

/// <summary>
/// Registers everything the Infrastructure layer provides.
/// </summary>
/// <remarks>
/// One registration point means the Web layer names no concrete store, so
/// swapping an implementation touches this file and nothing else.
/// </remarks>
public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<ServiceStandardsDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<ITaskStore, EfTaskStore>();
        services.AddScoped<ISignupStore, EfSignupStore>();
        services.AddScoped<IInventoryStore, EfInventoryStore>();
        services.AddScoped<IHealthProbe, DatabaseProbe>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<IEventDispatcher, EventDispatcher>();
        services.AddScoped<IEventConsumer, SignupRecordedConsumer>();
        services.AddScoped<OutboxPublisher>();
        return services;
    }

    /// <summary>
    /// Starts the background relay that publishes what the outbox holds.
    /// </summary>
    /// <remarks>
    /// Separate from AddInfrastructure because a process can need the stores
    /// without needing to publish: a test host that fakes them, or a replica
    /// that leaves publishing to one owner.
    /// </remarks>
    public static IServiceCollection AddOutboxRelay(this IServiceCollection services)
    {
        services.AddHostedService<OutboxRelay>();
        return services;
    }
}
