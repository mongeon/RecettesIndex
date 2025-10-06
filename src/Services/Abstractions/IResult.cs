namespace RecettesIndex.Services.Abstractions;

public interface IResult<out T>
{
    bool IsSuccess { get; }
    T? Value { get; }
    string? ErrorMessage { get; }
}
