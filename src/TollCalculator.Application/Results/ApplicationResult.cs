namespace TollCalculator.Application.Results;

public sealed record ApplicationResult<TValue>
{
    private ApplicationResult(TValue value)
    {
        IsSuccess = true;
        Value = value;
    }

    private ApplicationResult(string errorMessage, bool isNotFound)
    {
        ErrorMessage = errorMessage;
        IsNotFound = isNotFound;
    }

    public bool IsSuccess { get; }

    public bool IsNotFound { get; }

    public TValue? Value { get; }

    public string? ErrorMessage { get; }

    public static ApplicationResult<TValue> Success(TValue value)
    {
        return new ApplicationResult<TValue>(value);
    }

    public static ApplicationResult<TValue> NotFound(string message)
    {
        return WithError(message, isNotFound: true);
    }

    public static ApplicationResult<TValue> Invalid(string message)
    {
        return WithError(message, isNotFound: false);
    }

    private static ApplicationResult<TValue> WithError(string message, bool isNotFound)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("An error message is required.", nameof(message));
        }

        return new ApplicationResult<TValue>(message, isNotFound);
    }
}
