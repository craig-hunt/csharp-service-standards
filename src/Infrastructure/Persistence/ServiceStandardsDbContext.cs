using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ServiceStandards.Domain.Signups;
using ServiceStandards.Domain.Tasks;

namespace ServiceStandards.Infrastructure.Persistence;

/// <summary>
/// The relational model. Every table, column, and constraint is configured here
/// rather than through attributes on the row types.
/// </summary>
/// <remarks>
/// Configuration stays in one file so a reader sees the whole schema at once,
/// and the row types stay free of persistence attributes. The identifier
/// columns convert through typed identifiers, so a query cannot compare a task
/// identifier against a signup identifier even though both are bigint columns.
/// </remarks>
public sealed class ServiceStandardsDbContext(DbContextOptions<ServiceStandardsDbContext> options)
    : DbContext(options)
{
    public DbSet<TaskRow> Tasks => Set<TaskRow>();

    public DbSet<SignupRow> Signups => Set<SignupRow>();

    public DbSet<InventoryRow> InventoryItems => Set<InventoryRow>();

    public DbSet<OutboxRow> OutboxMessages => Set<OutboxRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // The read direction wraps without validating. EF runs the provider's
        // default through a key converter while it works out whether a key has
        // been set, so a validating factory here rejects zero and throws before
        // an insert reaches the database.
        var taskIdConverter = new ValueConverter<TaskId, long>(
            id => id.Value,
            value => TaskId.FromStored(value));

        var signupIdConverter = new ValueConverter<SignupId, long>(
            id => id.Value,
            value => SignupId.FromStored(value));

        modelBuilder.Entity<TaskRow>(entity =>
        {
            entity.ToTable(
                InfrastructureConstants.TableTasks,
                table => table.HasCheckConstraint(
                    InfrastructureConstants.CheckTasksTitle,
                    InfrastructureConstants.CheckTasksTitleSql));

            entity.HasKey(row => row.Id);

            entity.Property(row => row.Id)
                .HasColumnName(InfrastructureConstants.ColumnId)
                .HasConversion(taskIdConverter)
                .UseIdentityAlwaysColumn();

            entity.Property(row => row.Title)
                .HasColumnName(InfrastructureConstants.ColumnTitle)
                .IsRequired();

            entity.Property(row => row.Completed)
                .HasColumnName(InfrastructureConstants.ColumnCompleted)
                .HasDefaultValue(false);
        });

        modelBuilder.Entity<SignupRow>(entity =>
        {
            entity.ToTable(
                InfrastructureConstants.TableSignups,
                table =>
                {
                    table.HasCheckConstraint(
                        InfrastructureConstants.CheckSignupsPlan,
                        InfrastructureConstants.CheckSignupsPlanSql);
                    table.HasCheckConstraint(
                        InfrastructureConstants.CheckSignupsSeats,
                        InfrastructureConstants.CheckSignupsSeatsSql);
                });

            entity.HasKey(row => row.Id);

            entity.Property(row => row.Id)
                .HasColumnName(InfrastructureConstants.ColumnId)
                .HasConversion(signupIdConverter)
                .UseIdentityAlwaysColumn();

            entity.Property(row => row.FullName)
                .HasColumnName(InfrastructureConstants.ColumnFullName)
                .IsRequired();

            entity.Property(row => row.Email)
                .HasColumnName(InfrastructureConstants.ColumnEmail)
                .IsRequired();

            entity.Property(row => row.Plan)
                .HasColumnName(InfrastructureConstants.ColumnPlan)
                .IsRequired();

            entity.Property(row => row.Seats)
                .HasColumnName(InfrastructureConstants.ColumnSeats);

            entity.Property(row => row.Notes)
                .HasColumnName(InfrastructureConstants.ColumnNotes)
                .IsRequired()
                .HasDefaultValueSql(InfrastructureConstants.EmptyNotes);

            entity.Property(row => row.CreatedAt)
                .HasColumnName(InfrastructureConstants.ColumnCreatedAt)
                .HasDefaultValueSql(InfrastructureConstants.DefaultNowSql)
                .ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<InventoryRow>(entity =>
        {
            entity.ToTable(
                InfrastructureConstants.TableInventoryItems,
                table =>
                {
                    table.HasCheckConstraint(
                        InfrastructureConstants.CheckInventoryQuantity,
                        InfrastructureConstants.CheckInventoryQuantitySql);
                    table.HasCheckConstraint(
                        InfrastructureConstants.CheckInventoryStatus,
                        InfrastructureConstants.CheckInventoryStatusSql);
                });

            entity.HasKey(row => row.Name);

            entity.Property(row => row.Name)
                .HasColumnName(InfrastructureConstants.ColumnName);

            entity.Property(row => row.Quantity)
                .HasColumnName(InfrastructureConstants.ColumnQuantity);

            entity.Property(row => row.Status)
                .HasColumnName(InfrastructureConstants.ColumnStatus)
                .IsRequired();
        });

        modelBuilder.Entity<OutboxRow>(entity =>
        {
            entity.ToTable(InfrastructureConstants.TableOutbox);

            // The event identifier is the key, so redelivering a message the
            // producer already wrote cannot create a second row.
            entity.HasKey(row => row.EventId);

            entity.Property(row => row.EventId)
                .HasColumnName(InfrastructureConstants.ColumnEventId);

            entity.Property(row => row.Type)
                .HasColumnName(InfrastructureConstants.ColumnType)
                .IsRequired();

            entity.Property(row => row.Payload)
                .HasColumnName(InfrastructureConstants.ColumnPayload)
                .IsRequired();

            entity.Property(row => row.OccurredAt)
                .HasColumnName(InfrastructureConstants.ColumnOccurredAt);

            entity.Property(row => row.PublishedAt)
                .HasColumnName(InfrastructureConstants.ColumnPublishedAt);

            // The relay asks only for unpublished rows, so the index serves
            // exactly that query rather than the whole table.
            entity.HasIndex(row => row.PublishedAt)
                .HasDatabaseName(InfrastructureConstants.IndexOutboxPending);
        });
    }
}
