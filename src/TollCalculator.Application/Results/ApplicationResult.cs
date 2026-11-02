namespace TollCalculator.Application.Results;

public sealed record ApplicationResult<TValue>
{
    private ApplicationResult(TValue value)
    {
        Status = ApplicationResultStatus.Success;
        Value = value;
    }

    private ApplicationResult(ApplicationResultStatus status, string errorMessage)
    {
        Status = status;
        ErrorMessage = errorMessage;
    }

    public ApplicationResultStatus Status { get; }

    public TValue? Value { get; }

    public string? ErrorMessage { get; }

    public bool IsSuccess
    {
        get { return Status == ApplicationResultStatus.Success; }
    }

    public static ApplicationResult<TValue> Success(TValue value)
    {
        return new ApplicationResult<TValue>(value);
    }

    public static ApplicationResult<TValue> Invalid(string message)
    {
        return WithError(ApplicationResultStatus.ValidationError, message);
    }

    public static ApplicationResult<TValue> NotFound(string message)
    {
        return WithError(ApplicationResultStatus.NotFound, message);
    }

    public static ApplicationResult<TValue> Conflict(string message)
    {
        return WithError(ApplicationResultStatus.Conflict, message);
    }

    public static ApplicationResult<TValue> Error(string message)
    {
        return WithError(ApplicationResultStatus.Error, message);
    }

    private static ApplicationResult<TValue> WithError(ApplicationResultStatus status, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("An error message is required.", nameof(message));
        }

        return new ApplicationResult<TValue>(status, message);
    }
}
