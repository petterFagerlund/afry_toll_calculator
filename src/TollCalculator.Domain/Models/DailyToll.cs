namespace TollCalculator.Domain.Models;

public sealed record DailyToll
{
    public DailyToll(int totalFee, ChargedTollPassage[] chargedPassages)
    {
        ArgumentNullException.ThrowIfNull(chargedPassages);

        TotalFee = totalFee;
        ChargedPassages = chargedPassages;
    }

    public int TotalFee { get; }

    public ChargedTollPassage[] ChargedPassages { get; }

    public static DailyToll None()
    {
        return new DailyToll(0, []);
    }

    public bool Equals(DailyToll? other)
    {
        if (other is null)
        {
            return false;
        }

        return TotalFee == other.TotalFee && ChargedPassages.SequenceEqual(other.ChargedPassages);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();

        hash.Add(TotalFee);

        foreach (var chargedPassage in ChargedPassages)
        {
            hash.Add(chargedPassage);
        }

        return hash.ToHashCode();
    }
}
