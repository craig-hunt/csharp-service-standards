using System.Globalization;
using System.Text;
using ServiceStandards.Domain.Events;

namespace ServiceStandards.Domain.Signups;

/// <summary>
/// What the form submitted, before validation.
/// </summary>
/// <remarks>
/// Seats stays nullable so an omitted count takes the default while an explicit
/// zero still fails. Collapsing both to zero would reject a form that never
/// mentioned seats.
/// </remarks>
public sealed record SignupRequest(
    string? FullName,
    string? Email,
    string? Plan,
    int? Seats,
    string? Notes,
    bool AcceptTerms);

/// <summary>
/// A signup that passed validation, so every field already holds a usable value.
/// </summary>
public sealed record Signup(string FullName, string Email, Plan Plan, int Seats, string Notes)
{
    private static readonly CompositeFormat SummaryTemplate =
        CompositeFormat.Parse(SignupConstants.SummaryFormat);

    public string Summary() => string.Format(
        CultureInfo.InvariantCulture,
        SummaryTemplate,
        FullName,
        Plan.Value,
        Seats);

    /// <summary>
    /// Describes this signup as the event a consumer receives.
    /// </summary>
    /// <remarks>
    /// The signup composes its own event, so the fact travels with the rules
    /// that produced it rather than being assembled by whichever layer happens
    /// to write the row. The identifier arrives from the store because the
    /// database assigns it.
    /// </remarks>
    public SignupRecorded Recorded(SignupId id, Guid eventId, DateTimeOffset occurredAt) =>
        new(eventId, occurredAt, id.Value, Email, Plan.Value, Seats);
}

/// <summary>
/// What a signup confirmation returns to the caller.
/// </summary>
public sealed record SignupConfirmation(SignupId Id, string Summary);
