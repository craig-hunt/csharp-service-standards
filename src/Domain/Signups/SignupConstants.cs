namespace ServiceStandards.Domain.Signups;

/// <summary>
/// Every literal the signup feature carries, named once.
/// </summary>
public static class SignupConstants
{
    public const string PlanStarter = "Starter";
    public const string PlanGrowth = "Growth";
    public const string PlanEnterprise = "Enterprise";

    public const string FieldFullName = "fullName";
    public const string FieldEmail = "email";
    public const string FieldPlan = "plan";
    public const string FieldSeats = "seats";
    public const string FieldNotes = "notes";
    public const string FieldAcceptTerms = "acceptTerms";

    public const string MsgNameRequired = "Enter your full name.";
    public const string MsgEmailRequired = "Enter your work email.";
    public const string MsgEmailInvalid = "Enter a valid email address.";
    public const string MsgPlanRequired = "Choose a plan.";
    public const string MsgPlanUnknown = "Choose the Starter, Growth, or Enterprise plan.";
    public const string MsgSeatsInvalid = "Enter at least one seat.";
    public const string MsgNotesTooLong = "Keep the notes within the length limit.";
    public const string MsgTermsRequired = "Accept the terms to continue.";

    public const string CodeInvalidId = "invalid_id";
    public const string MsgInvalidId = "signup id must be a positive whole number";

    public const int DefaultSeats = 1;
    public const int MinSeats = 1;
    public const int MaxNotesLength = 1000;

    public const string EmailPattern = @"^[^\s@]+@[^\s@]+\.[^\s@]+$";
    public const string SummaryFormat = "{0} on the {1} plan, {2} seat(s).";
}
