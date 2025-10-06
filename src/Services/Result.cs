using RecettesIndex.Services.Abstractions;

namespace RecettesIndex.Services;

public sealed class Result<T> : IResult<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? ErrorMessage { get; }

    private Result(bool isSuccess, T? value, string? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorMessage = error;
    }

    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Failure(string message) => new(false, default, message);
}
