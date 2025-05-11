namespace Uptime.Workflows.Core.Common;

public enum ErrorCode
{
    // 400s – client / validation errors
    ValidationFailed,   // 400 Bad Request
    Forbidden,          // 403 Forbidden
    NotFound,           // 404 Not Found
    Unsupported,        // 405 Method Not Allowed
    Conflict,           // 409 Conflict

    // other application‐specific
    Cancelled,          // e.g. client‐cancellation

    // 500s – server / unexpected errors
    Unexpected          // 500 Internal Server Error
}

public readonly record struct Unit;

public readonly record struct Result<TValue>
{
    public bool Succeeded { get; }
    public TValue? Value { get; }
    public ErrorCode? Code { get; }
    public string? Details { get; }
    
    private Result(bool succeeded, TValue? value, ErrorCode? code = null, string? details = null)
    {
        Succeeded = succeeded;
        Value = value;
        Code = code;
        Details = details;
    }

    public static Result<TValue> Success(TValue? value) => new(true, value);
    public static Result<TValue> Failure(ErrorCode code, string? details = null) => new(false, default, code, details);
    public static Result<TValue> Cancelled() => Failure(ErrorCode.Cancelled);
}