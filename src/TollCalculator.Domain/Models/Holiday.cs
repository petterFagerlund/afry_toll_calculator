namespace TollCalculator.Domain.Models;

public sealed record Holiday
{
    private Holiday(DateOnly date, string name)
    {
        Date = date;
        Name = name;
    }

    public DateOnly Date { get; }

    public string Name { get; }

    public static Holiday Create(DateOnly date, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("A holiday name is required.", nameof(name));
        }

        return new Holiday(date, name.Trim());
    }
}
