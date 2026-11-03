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

    public bool IsSuccess => Status == ApplicationResultStatus.Success;

    public ApplicationResult<TNext> Map<TNext>(Func<TValue, TNext> map)
    {
        ArgumentNullException.ThrowIfNull(map);

        if (!IsSuccess)
        {
            return ApplicationResult<TNext>.WithError(Status, ErrorMessage!);
        }

        return ApplicationResult<TNext>.Success(map(Value!));
    }

    public static ApplicationResult<TValue> Success(TValue value)
    {
        return new ApplicationResult<TValue>(value);
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
