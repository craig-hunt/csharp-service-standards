using ServiceStandards.Domain.Signups;
using Xunit;

namespace ServiceStandards.Domain.Tests.Signups;

/// <summary>
/// Signup validation, including the rule that every problem reports at once.
/// </summary>
/// <remarks>
/// The all-at-once rule is the one worth guarding hardest: a validator that
/// returns on its first problem still passes every single-field test, and only
/// an assertion over a wholly empty request catches it.
/// </remarks>
public sealed class SignupValidatorTests
{
    private const string ValidName = "Dana Reyes";
    private const string ValidEmail = "dana@example.com";
    private const string InvalidEmail = "dana at example.com";
    private const string UnknownPlan = "Platinum";
    private const string Notes = "Migrating from a legacy system.";
    private const string Untrimmed = "   Dana Reyes   ";
    private const int ExplicitZeroSeats = 0;
    private const int ExplicitSeats = 12;
    private const int EmptyRequestProblems = 4;
    private const char Filler = 'n';

    [Fact]
    public void ValidateReportsEveryProblemAtOnce()
    {
        var validation = SignupValidator.Validate(new SignupRequest(null, null, null, null, null, false));

        Assert.Equal(EmptyRequestProblems, validation.Problems.Count);
        Assert.Equal(SignupConstants.MsgNameRequired, validation.Problems[SignupConstants.FieldFullName]);
        Assert.Equal(SignupConstants.MsgEmailRequired, validation.Problems[SignupConstants.FieldEmail]);
        Assert.Equal(SignupConstants.MsgPlanRequired, validation.Problems[SignupConstants.FieldPlan]);
        Assert.Equal(SignupConstants.MsgTermsRequired, validation.Problems[SignupConstants.FieldAcceptTerms]);
    }

    [Fact]
    public void ValidateAcceptsACompleteRequest()
    {
        var validation = SignupValidator.Validate(Valid());

        Assert.True(validation.Valid);
        Assert.NotNull(validation.Value);
        Assert.Empty(validation.Problems);
    }

    [Fact]
    public void ValidateTrimsTheName()
    {
        var validation = SignupValidator.Validate(Valid() with { FullName = Untrimmed });

        Assert.Equal(ValidName, validation.Value?.FullName);
    }

    [Fact]
    public void ValidateRejectsAMalformedEmail()
    {
        var validation = SignupValidator.Validate(Valid() with { Email = InvalidEmail });

        Assert.Equal(SignupConstants.MsgEmailInvalid, validation.Problems[SignupConstants.FieldEmail]);
    }

    [Fact]
    public void ValidateRejectsAnUnknownPlan()
    {
        var validation = SignupValidator.Validate(Valid() with { Plan = UnknownPlan });

        Assert.Equal(SignupConstants.MsgPlanUnknown, validation.Problems[SignupConstants.FieldPlan]);
    }

    [Theory]
    [InlineData("Starter")]
    [InlineData("Growth")]
    [InlineData("Enterprise")]
    public void ValidateAcceptsEveryKnownPlan(string plan)
    {
        var validation = SignupValidator.Validate(Valid() with { Plan = plan });

        Assert.True(validation.Valid);
    }

    [Fact]
    public void ValidateDefaultsOmittedSeats()
    {
        var validation = SignupValidator.Validate(Valid() with { Seats = null });

        Assert.Equal(SignupConstants.DefaultSeats, validation.Value?.Seats);
    }

    [Fact]
    public void ValidateRejectsAnExplicitZeroSeats()
    {
        var validation = SignupValidator.Validate(Valid() with { Seats = ExplicitZeroSeats });

        Assert.Equal(SignupConstants.MsgSeatsInvalid, validation.Problems[SignupConstants.FieldSeats]);
    }

    [Fact]
    public void ValidateKeepsAnExplicitSeatCount()
    {
        var validation = SignupValidator.Validate(Valid() with { Seats = ExplicitSeats });

        Assert.Equal(ExplicitSeats, validation.Value?.Seats);
    }

    [Fact]
    public void ValidateAcceptsNotesAtTheLimit()
    {
        var atLimit = new string(Filler, SignupConstants.MaxNotesLength);

        var validation = SignupValidator.Validate(Valid() with { Notes = atLimit });

        Assert.True(validation.Valid);
    }

    [Fact]
    public void ValidateRejectsNotesBeyondTheLimit()
    {
        var beyondLimit = new string(Filler, SignupConstants.MaxNotesLength + 1);

        var validation = SignupValidator.Validate(Valid() with { Notes = beyondLimit });

        Assert.Equal(SignupConstants.MsgNotesTooLong, validation.Problems[SignupConstants.FieldNotes]);
    }

    [Fact]
    public void ValidateRejectsUnacceptedTerms()
    {
        var validation = SignupValidator.Validate(Valid() with { AcceptTerms = false });

        Assert.Equal(SignupConstants.MsgTermsRequired, validation.Problems[SignupConstants.FieldAcceptTerms]);
    }

    [Fact]
    public void SummaryNamesThePersonPlanAndSeats()
    {
        var validation = SignupValidator.Validate(Valid() with { Seats = ExplicitSeats });

        Assert.Contains(ValidName, validation.Value?.Summary(), StringComparison.Ordinal);
        Assert.Contains(SignupConstants.PlanStarter, validation.Value?.Summary(), StringComparison.Ordinal);
    }

    private static SignupRequest Valid() =>
        new(ValidName, ValidEmail, SignupConstants.PlanStarter, null, Notes, true);
}
