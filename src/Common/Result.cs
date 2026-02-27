namespace AHS.Common;

public class Result
{
    protected Result(bool isSuccess, string error)
    {
        if (isSuccess && !string.IsNullOrEmpty(error)) throw new InvalidOperationException();
        if (!isSuccess && string.IsNullOrEmpty(error)) throw new InvalidOperationException();
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }

    public static Result Success() => new(true, string.Empty);
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, string.Empty);
    public static Result Failure(string error) => new(false, error);
    public static Result<TValue> Failure<TValue>(string error) => new(default!, false, error);

    /// <summary>Named alternative for the implicit conversion operator (CA2225). Placed here to avoid CA1000.</summary>
    public static Result<TValue> FromValue<TValue>(TValue? value) => Success<TValue>(value!);
}

public class Result<TValue> : Result
{
    private readonly TValue? _value;

    protected internal Result(TValue? value, bool isSuccess, string error)
        : base(isSuccess, error) => _value = value;

    public TValue Value => IsSuccess ? _value! : throw new InvalidOperationException();

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2225:Operator overloads have named alternates", Justification = "Named alternate Result.FromValue<TValue>() is on the non-generic base class to satisfy CA1000.")]
    public static implicit operator Result<TValue>(TValue? value) => FromValue(value);
}
