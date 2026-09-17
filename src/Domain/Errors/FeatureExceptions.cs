using System.Collections.ObjectModel;
using ServiceStandards.Domain.Inventory;
using ServiceStandards.Domain.Signups;
using ServiceStandards.Domain.Tasks;

namespace ServiceStandards.Domain.Errors;

/// <summary>Raised when a task update omits the completion flag.</summary>
public sealed class TaskCompletedRequiredException : ValidationException
{
    public TaskCompletedRequiredException()
        : base(ErrorConstants.CodeValidation, ErrorConstants.MsgValidation, FieldProblem())
    {
    }

    private static ReadOnlyDictionary<string, string> FieldProblem() =>
        new(new Dictionary<string, string>
        {
            [TaskConstants.FieldCompleted] = TaskConstants.MsgCompletedRequired,
        });
}

/// <summary>Raised when a submitted signup has field problems.</summary>
public sealed class SignupInvalidException : ValidationException
{
    public SignupInvalidException(IReadOnlyDictionary<string, string> problems)
        : base(ErrorConstants.CodeValidation, ErrorConstants.MsgValidation, problems)
    {
    }
}

/// <summary>Raised when a stock query names a column the table cannot sort on.</summary>
public sealed class InvalidSortException : ValidationException
{
    public InvalidSortException()
        : base(InventoryConstants.CodeInvalidQuery, InventoryConstants.MsgInvalidSort)
    {
    }
}

/// <summary>Raised when a stock query names an order that does not exist.</summary>
public sealed class InvalidDirectionException : ValidationException
{
    public InvalidDirectionException()
        : base(InventoryConstants.CodeInvalidQuery, InventoryConstants.MsgInvalidDirection)
    {
    }
}

/// <summary>Raised when a signup identifier falls below the first valid value.</summary>
public sealed class InvalidSignupIdException : ValidationException
{
    public InvalidSignupIdException()
        : base(SignupConstants.CodeInvalidId, SignupConstants.MsgInvalidId)
    {
    }
}
