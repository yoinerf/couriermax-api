namespace CourierMax.Application.Common;

/// <summary>
/// Patrón Result: envuelve el éxito/fallo sin lanzar excepciones para el flujo de control.
/// </summary>
public class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public string? Error { get; }
    public string? ErrorCode { get; }

    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
    }

    private Result(string error, string? errorCode = null)
    {
        IsSuccess = false;
        Error = error;
        ErrorCode = errorCode;
    }

    public static Result<T> Success(T value) => new(value);

    public static Result<T> Failure(string error, string? errorCode = null) => new(error, errorCode);

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, string?, TResult> onFailure)
        => IsSuccess ? onSuccess(Value!) : onFailure(Error!, ErrorCode);
}

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }
    public string? ErrorCode { get; }

    private Result(bool isSuccess, string? error = null, string? errorCode = null)
    {
        IsSuccess = isSuccess;
        Error = error;
        ErrorCode = errorCode;
    }

    public static Result Success() => new(true);
    public static Result Failure(string error, string? errorCode = null) => new(false, error, errorCode);
}
