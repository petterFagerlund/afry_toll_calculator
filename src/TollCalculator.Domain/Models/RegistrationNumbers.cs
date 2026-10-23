namespace TollCalculator.Domain.Models;

public static class RegistrationNumbers
{
    public static string Normalize(string registrationNumber, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(registrationNumber))
        {
            throw new ArgumentException("A registration number is required.", parameterName);
        }

        return registrationNumber.Trim().ToUpperInvariant();
    }
}
