namespace OpenAuth.Application.Shared.Models;

public class Result<T>
{
    public T Value { get; }
    public bool IsSuccess { get; }
    public Error? Error { get; }
    
    private Result(T value, bool isSuccess, Error? error)
    {
        Value = value;
        IsSuccess = isSuccess;
        Error = error;
    }
    
    public static Result<T> Ok(T value)
        => new(value, true, null);

    public static Result<T> Fail(Error error)
        => new (default!, false, error);
}