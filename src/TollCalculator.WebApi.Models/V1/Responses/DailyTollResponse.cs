namespace TollCalculator.WebApi.Models.V1.Responses;

public sealed record DailyTollResponse
{
    public required string RegistrationNumber { get; init; }

    public required DateOnly Date { get; init; }

    public required int TotalFee { get; init; }

    public ChargedTollPassage[] ChargedPassages { get; init; } = [];

    public bool Equals(DailyTollResponse? other)
    {
        if (other is null)
        {
            return false;
        }

        return RegistrationNumber == other.RegistrationNumber
            && Date == other.Date
            && TotalFee == other.TotalFee
            && ChargedPassages.SequenceEqual(other.ChargedPassages);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();

        hash.Add(RegistrationNumber);
        hash.Add(Date);
        hash.Add(TotalFee);

        foreach (var chargedPassage in ChargedPassages)
        {
            hash.Add(chargedPassage);
        }

        return hash.ToHashCode();
    }
}
