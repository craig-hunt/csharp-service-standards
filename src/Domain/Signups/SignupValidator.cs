using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using ServiceStandards.Domain.Text;

namespace ServiceStandards.Domain.Signups;

/// <summary>
/// The outcome of validating a signup: either the validated signup, or the
/// problems keyed by the field each one belongs to.
/// </summary>
public sealed record SignupValidation(Signup? Value, IReadOnlyDictionary<string, string> Problems)
{
    public bool Valid => Problems.Count == 0;
}

/// <summary>
/// Validates a submitted signup, reporting every field problem at once.
/// </summary>
/// <remarks>
/// Returning on the first problem would make a form correct one field per round
/// trip. Every check runs, so a caller marks all of them together.
/// </remarks>
public static partial class SignupValidator
{
    private const int NoProblems = 0;

    public static SignupValidation Validate(SignupRequest request)
    {
        var problems = new Dictionary<string, string>();

        var name = (request.FullName ?? string.Empty).Trim();
        if (name.Length == NoProblems)
        {
            problems[SignupConstants.FieldFullName] = SignupConstants.MsgNameRequired;
        }

        var email = (request.Email ?? string.Empty).Trim();
        if (email.Length == NoProblems)
        {
            problems[SignupConstants.FieldEmail] = SignupConstants.MsgEmailRequired;
        }
        else if (!EmailPattern().IsMatch(email))
        {
            problems[SignupConstants.FieldEmail] = SignupConstants.MsgEmailInvalid;
        }

        var plan = Plan.From(request.Plan);
        if (plan.IsAbsent)
        {
            problems[SignupConstants.FieldPlan] = SignupConstants.MsgPlanRequired;
        }
        else if (!plan.Known)
        {
            problems[SignupConstants.FieldPlan] = SignupConstants.MsgPlanUnknown;
        }

        var seats = SignupConstants.DefaultSeats;
        if (request.Seats is not null)
        {
            seats = request.Seats.Value;
            if (seats < SignupConstants.MinSeats)
            {
                problems[SignupConstants.FieldSeats] = SignupConstants.MsgSeatsInvalid;
            }
        }

        var notes = (request.Notes ?? string.Empty).Trim();
        if (TextLength.CountRunes(notes) > SignupConstants.MaxNotesLength)
        {
            problems[SignupConstants.FieldNotes] = SignupConstants.MsgNotesTooLong;
        }

        if (!request.AcceptTerms)
        {
            problems[SignupConstants.FieldAcceptTerms] = SignupConstants.MsgTermsRequired;
        }

        var reported = new ReadOnlyDictionary<string, string>(problems);
        return problems.Count > NoProblems
            ? new SignupValidation(null, reported)
            : new SignupValidation(new Signup(name, email, plan, seats, notes), reported);
    }

    [GeneratedRegex(SignupConstants.EmailPattern)]
    private static partial Regex EmailPattern();
}
