using TollCalculator.WebApi.Models.V1.Requests;

namespace TollCalculator.WebApi.Host.Validators;

public interface IRecordTollPassageRequestValidator
{
    ValidationResult Validate(RecordTollPassageRequest? request);
}
