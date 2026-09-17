namespace ServiceStandards.Infrastructure;

/// <summary>
/// The table names, column names, and constraints the relational model uses.
/// </summary>
/// <remarks>
/// The check-constraint SQL repeats the numbers and words the domain constants
/// already carry, because a constraint expression has to reach the database as
/// one literal string. A unit test pins each of these against its domain
/// constant, so a limit changed in one place fails the build rather than
/// drifting quietly apart.
/// </remarks>
public static class InfrastructureConstants
{
    public const string ConnectionName = "Default";

    public const string TableTasks = "tasks";
    public const string TableSignups = "signups";
    public const string TableInventoryItems = "inventory_items";

    public const string ColumnId = "id";
    public const string ColumnTitle = "title";
    public const string ColumnCompleted = "completed";
    public const string ColumnFullName = "full_name";
    public const string ColumnEmail = "email";
    public const string ColumnPlan = "plan";
    public const string ColumnSeats = "seats";
    public const string ColumnNotes = "notes";
    public const string ColumnCreatedAt = "created_at";
    public const string ColumnName = "name";
    public const string ColumnQuantity = "quantity";
    public const string ColumnStatus = "status";

    public const string CheckTasksTitle = "ck_tasks_title_length";
    public const string CheckSignupsPlan = "ck_signups_plan";
    public const string CheckSignupsSeats = "ck_signups_seats";
    public const string CheckInventoryQuantity = "ck_inventory_items_quantity";
    public const string CheckInventoryStatus = "ck_inventory_items_status";

    public const string CheckTasksTitleSql = "char_length(title) BETWEEN 1 AND 200";
    public const string CheckSignupsPlanSql = "plan IN ('Starter', 'Growth', 'Enterprise')";
    public const string CheckSignupsSeatsSql = "seats >= 1";
    public const string CheckInventoryQuantitySql = "quantity >= 0";
    public const string CheckInventoryStatusSql = "status IN ('In stock', 'Low', 'Out of stock')";

    public const string TableOutbox = "outbox_messages";
    public const string ColumnEventId = "event_id";
    public const string ColumnType = "type";
    public const string ColumnPayload = "payload";
    public const string ColumnOccurredAt = "occurred_at";
    public const string ColumnPublishedAt = "published_at";
    public const string IndexOutboxPending = "ix_outbox_messages_published_at";

    public const int OutboxBatchSize = 50;
    public const int OutboxPollSeconds = 2;
    public const string MsgOutboxPublishFailed = "outbox publish failed";

    // Every argument a logging message carries has to appear in its template.
    // The generator rejects a parameter the message never names, which is what
    // keeps a structured log from quietly dropping the fields it was given.
    public const string MsgSignupRecorded = "signup recorded: {SignupId} on {Plan} for {Seats} seats";

    public const string DefaultNowSql = "now()";
    public const string PingSql = "SELECT 1";
    public const string EmptyNotes = "''";
}
