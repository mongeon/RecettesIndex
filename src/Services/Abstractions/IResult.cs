namespace RecettesIndex.Services.Abstractions;

public interface IResult<out T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? ErrorMessage { get; }
}
