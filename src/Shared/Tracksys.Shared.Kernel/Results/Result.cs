namespace Tracksys.Shared.Kernel.Results;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }

    protected Result(bool isSuccess, string? error)
    {
        if (isSuccess && error is not null)
            throw new InvalidOperationException("Un résultat réussi ne peut pas porter d'erreur.");
        if (!isSuccess && error is null)
            throw new InvalidOperationException("Un résultat en échec doit porter une erreur.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, null);
    public static Result Failure(string error) => new(false, error);

    public static Result<T> Success<T>(T value) => new(value, true, null);
    public static Result<T> Failure<T>(string error) => new(default, false, error);
}

public class Result<T> : Result
{
    private readonly T? _value;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Impossible de lire Value sur un Result en échec.");

    protected internal Result(T? value, bool isSuccess, string? error) : base(isSuccess, error)
    {
        _value = value;
    }

    public static implicit operator Result<T>(T value) => Success(value);
}
