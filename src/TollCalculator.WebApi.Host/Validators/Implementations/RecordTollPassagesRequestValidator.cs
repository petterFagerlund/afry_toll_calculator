using Microsoft.AspNetCore.Mvc.ModelBinding;
using TollCalculator.WebApi.Models.V1.Requests;

namespace TollCalculator.WebApi.Host.Validators.Implementations;

internal static class RecordTollPassagesRequestValidator
{
    private const string PassagesMember = "passages";

    // One request is one roadside batch. The cap keeps a single POST from loading an unbounded number of
    // entities into the change tracker and holding one transaction open across all of them.
    private const int MaximumPassages = 1000;

    public static ModelStateDictionary Validate(RecordTollPassagesRequest? request, DateTimeOffset now)
    {
        var modelState = new ModelStateDictionary();

        if (request is null)
        {
            modelState.AddModelError(PassagesMember, "A request body is required.");

            return modelState;
        }

        if (request.Passages.Length == 0)
        {
            modelState.AddModelError(PassagesMember, "At least one passage is required.");

            return modelState;
        }

        if (request.Passages.Length > MaximumPassages)
        {
            modelState.AddModelError(
                PassagesMember,
                $"A batch cannot hold more than {MaximumPassages} passages; {request.Passages.Length} were sent.");

            return modelState;
        }

        for (var index = 0; index < request.Passages.Length; index++)
        {
            ValidatePassage(request.Passages[index], index, now, modelState);
        }

        return modelState;
    }

    private static ModelStateDictionary ValidatePassage(
        TollPassageToRecord passage,
        int index,
        DateTimeOffset now,
        ModelStateDictionary modelState)
    {
        var registrationNumberError = RegistrationNumberRule.Check(passage.RegistrationNumber);
        if (registrationNumberError is not null)
        {
            modelState.AddModelError(MemberAt(index, nameof(TollPassageToRecord.RegistrationNumber)), registrationNumberError);
        }

        var vehicleTypeError = VehicleTypeRule.Check(passage.VehicleType);
        if (vehicleTypeError is not null)
        {
            modelState.AddModelError(MemberAt(index, nameof(TollPassageToRecord.VehicleType)), vehicleTypeError);
        }

        var passedAtError = PassageTimestampRule.Check(passage.PassedAt, now);
        if (passedAtError is not null)
        {
            modelState.AddModelError(MemberAt(index, nameof(TollPassageToRecord.PassedAt)), passedAtError);
        }

        return modelState;
    }

    private static string MemberAt(int index, string member)
    {
        return $"{PassagesMember}[{index}].{member}";
    }
}
