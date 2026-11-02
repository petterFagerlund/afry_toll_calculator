namespace TollCalculator.Domain.Models;

public static class RegistrationNumbers
{
    public static string Normalize(string registrationNumber)
    {
        if (string.IsNullOrWhiteSpace(registrationNumber))
        {
            throw new ArgumentException("A registration number is required.");
        }

        return registrationNumber.Trim().ToUpperInvariant();
    }
}
