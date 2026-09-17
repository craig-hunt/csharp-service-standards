namespace ServiceStandards.Domain.Tasks;

/// <summary>
/// Every literal the task feature carries, named once. Tests reference these
/// rather than repeating the values, so a reworded message fails at compile
/// time instead of drifting.
/// </summary>
public static class TaskConstants
{
    public const int MaxTitleLength = 200;

    public const string FilterAll = "all";
    public const string FilterActive = "active";
    public const string FilterCompleted = "completed";

    public const string FieldTitle = "title";
    public const string FieldCompleted = "completed";

    public const string CodeInvalidFilter = "invalid_filter";
    public const string CodeInvalidId = "invalid_id";
    public const string CodeNotFound = "not_found";

    public const string MsgInvalidFilter = "filter must be all, active, or completed";
    public const string MsgInvalidId = "task id must be a positive whole number";
    public const string MsgNotFound = "no task has that id";
    public const string MsgTitleRequired = "Enter a task title.";
    public const string MsgTitleTooLong = "Keep the task title within the length limit.";
    public const string MsgCompletedRequired = "Say whether the task is completed.";

    public const string SeedTitleFirst = "Review the architecture decision record";
    public const string SeedTitleSecond = "Reply to the vendor questionnaire";
}
