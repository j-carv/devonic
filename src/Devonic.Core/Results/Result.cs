namespace Devonic.Core.Results;

public sealed class Result<T>
{
    public T? Value { get; }
    public string? Error { get; }
    public bool IsSuccess => Error is null;

    private Result(T value) => Value = value;
    private Result(string error) => Error = error;

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(string error) => new(error);
}

public sealed class Result
{
    public string? Error { get; }
    public bool IsSuccess => Error is null;

    private Result() { }
    private Result(string error) => Error = error;

    public static Result Success() => new();
    public static Result Failure(string error) => new(error);
}
