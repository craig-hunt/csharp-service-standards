using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ServiceStandards.Domain.Tasks;

namespace ServiceStandards.Domain.Errors;

/// <summary>
/// The base every domain failure derives from, carrying a stable code and the
/// per-field problems an API can render.
/// </summary>
/// <remarks>
/// Middleware translates this hierarchy to RFC 9457 problem details in one
/// place, which is why no endpoint here carries a try/catch. A code travels with
/// the exception because callers match on identity, never on message text.
/// </remarks>
public abstract class DomainException : Exception
{
    private static readonly IReadOnlyDictionary<string, string> NoFields =
        new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());

    protected DomainException(string code, string message)
        : this(code, message, NoFields)
    {
    }

    protected DomainException(string code, string message, IReadOnlyDictionary<string, string> fields)
        : base(message)
    {
        Code = code;
        Fields = fields;
    }

    public string Code { get; }

    public IReadOnlyDictionary<string, string> Fields { get; }
}

/// <summary>Raised when a request names a resource that does not exist.</summary>
public abstract class NotFoundException : DomainException
{
    protected NotFoundException(string code, string message)
        : base(code, message)
    {
    }
}

/// <summary>Raised when a request carries a value the domain rejects.</summary>
public abstract class ValidationException : DomainException
{
    protected ValidationException(string code, string message)
        : base(code, message)
    {
    }

    protected ValidationException(string code, string message, IReadOnlyDictionary<string, string> fields)
        : base(code, message, fields)
    {
    }
}

public sealed class TaskTitleRequiredException : ValidationException
{
    public TaskTitleRequiredException()
        : base(ErrorConstants.CodeValidation, ErrorConstants.MsgValidation, FieldProblem())
    {
    }

    private static ReadOnlyDictionary<string, string> FieldProblem() =>
        new(new Dictionary<string, string>
        {
            [TaskConstants.FieldTitle] = TaskConstants.MsgTitleRequired,
        });
}

public sealed class TaskTitleTooLongException : ValidationException
{
    public TaskTitleTooLongException()
        : base(ErrorConstants.CodeValidation, ErrorConstants.MsgValidation, FieldProblem())
    {
    }

    private static ReadOnlyDictionary<string, string> FieldProblem() =>
        new(new Dictionary<string, string>
        {
            [TaskConstants.FieldTitle] = TaskConstants.MsgTitleTooLong,
        });
}

public sealed class InvalidTaskFilterException : ValidationException
{
    public InvalidTaskFilterException()
        : base(TaskConstants.CodeInvalidFilter, TaskConstants.MsgInvalidFilter)
    {
    }
}

public sealed class InvalidTaskIdException : ValidationException
{
    public InvalidTaskIdException()
        : base(TaskConstants.CodeInvalidId, TaskConstants.MsgInvalidId)
    {
    }
}

public sealed class TaskNotFoundException : NotFoundException
{
    public TaskNotFoundException()
        : base(TaskConstants.CodeNotFound, TaskConstants.MsgNotFound)
    {
    }
}
