using FluentValidation;
using PayorClaims.Application.Dtos.Claims;

namespace PayorClaims.Application.Validation;

public class ClaimLineInputValidator : AbstractValidator<ClaimLineInputDto>
{
    public ClaimLineInputValidator()
    {
        RuleFor(x => x.LineNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.CptCode).NotEmpty().MaximumLength(10);
        RuleFor(x => x.Units).InclusiveBetween(1, 999);
        RuleFor(x => x.BilledAmount).GreaterThan(0);
        When(x => x.Diagnoses != null, () =>
        {
            RuleForEach(x => x.Diagnoses).SetValidator(new ClaimDiagnosisInputValidator());
        });
    }
}
