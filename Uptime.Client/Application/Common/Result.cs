using System.Diagnostics.CodeAnalysis;

namespace Uptime.Client.Application.Common;

public readonly record struct Result<TValue>
{
    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool Succeeded { get; }
    public TValue? Value { get; }
    public string Error { get; }

    private Result(TValue? value, string error, bool succeeded)
    {
        Value = value;
        Error = error;
        Succeeded = succeeded;
    }

    public bool TryGetValue(out TValue? value, out string error)
    {
        value = Value;
        error = Error;

        return Succeeded;
    }

    public static Result<TValue> Success(TValue? value) => new(value, null!, true);
    public static Result<TValue> Failure(string error) => new(default!, error, false);
    public static Result<TValue> Failure(IEnumerable<string> errors) => new(default!, string.Join("\n", errors), false);
    public static Result<TValue> Cancelled() => new(default!, "Operation cancelled", false);
}