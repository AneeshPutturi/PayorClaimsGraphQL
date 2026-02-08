using FluentValidation;
using PayorClaims.Application.Dtos.Claims;

namespace PayorClaims.Application.Validation;

public class ClaimDiagnosisInputValidator : AbstractValidator<ClaimDiagnosisInputDto>
{
    public ClaimDiagnosisInputValidator()
    {
        RuleFor(x => x.CodeSystem).Equal("ICD10").WithMessage("codeSystem must be ICD10");
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
    }
}
