namespace ServiceStandards.Domain.Signups;

/// <summary>
/// The plan a signup names, kept as the value the form submitted.
/// </summary>
/// <remarks>
/// An enum would collapse an absent plan and an unrecognized one into the same
/// undefined member, and the form shows a different sentence for each. Holding
/// the submitted text keeps that distinction available to the validator.
/// </remarks>
public readonly record struct Plan
{
    private Plan(string value) => Value = value;

    public string Value { get; }

    public bool IsAbsent => string.IsNullOrEmpty(Value);

    public bool Known =>
        Value == SignupConstants.PlanStarter
        || Value == SignupConstants.PlanGrowth
        || Value == SignupConstants.PlanEnterprise;

    public static Plan From(string? raw) => new(raw ?? string.Empty);

    public override string ToString() => Value;
}
