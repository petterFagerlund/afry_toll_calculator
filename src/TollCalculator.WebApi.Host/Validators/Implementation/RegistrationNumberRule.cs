namespace TollCalculator.WebApi.Host.Validators.Implementation;

internal static class RegistrationNumberRule
{
    public const int MaximumLength = 20;

    public static string? Check(string? registrationNumber)
    {
        if (string.IsNullOrWhiteSpace(registrationNumber))
        {
            return "A registration number is required.";
        }

        if (registrationNumber.Trim().Length > MaximumLength)
        {
            return $"A registration number cannot be longer than {MaximumLength} characters.";
        }

        return null;
    }
}
