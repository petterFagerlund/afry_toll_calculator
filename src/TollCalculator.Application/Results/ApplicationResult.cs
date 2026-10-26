namespace TollCalculator.Application.Results;

public sealed record ApplicationResult<TValue>
{
    private ApplicationResult(TValue value)
    {
        IsSuccess = true;
        Value = value;
    }

    private ApplicationResult(string errorMessage, bool isNotFound, bool isInvalid)
    {
        ErrorMessage = errorMessage;
        IsNotFound = isNotFound;
        IsInvalid = isInvalid;
    }

    public bool IsSuccess { get; }

    public bool IsNotFound { get; }

    public bool IsInvalid { get; }

    public TValue? Value { get; }

    public string? ErrorMessage { get; }

    public static ApplicationResult<TValue> Success(TValue value)
    {
        return new ApplicationResult<TValue>(value);
    }

    public static ApplicationResult<TValue> NotFound(string message)
    {
        return WithError(message, isNotFound: true, isInvalid: false);
    }

    public static ApplicationResult<TValue> Invalid(string message)
    {
        return WithError(message, isNotFound: false, isInvalid: true);
    }

    private static ApplicationResult<TValue> WithError(string message, bool isNotFound, bool isInvalid)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("An error message is required.", nameof(message));
        }

        return new ApplicationResult<TValue>(message, isNotFound, isInvalid);
    }
}
