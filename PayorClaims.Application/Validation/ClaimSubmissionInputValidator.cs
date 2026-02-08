using FluentValidation;
using PayorClaims.Application.Dtos.Claims;

namespace PayorClaims.Application.Validation;

public class ClaimSubmissionInputValidator : AbstractValidator<ClaimSubmissionInputDto>
{
    public ClaimSubmissionInputValidator()
    {
        RuleFor(x => x.MemberId).NotEmpty();
        RuleFor(x => x.ProviderId).NotEmpty();
        RuleFor(x => x.ServiceTo).GreaterThanOrEqualTo(x => x.ServiceFrom).WithMessage("serviceTo must be >= serviceFrom");
        RuleFor(x => x.ReceivedDate)
            .GreaterThanOrEqualTo(x => x.ServiceFrom.AddDays(-30))
            .WithMessage("receivedDate must be on or after serviceFrom - 30 days");
        RuleFor(x => x.ReceivedDate)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1))
            .WithMessage("receivedDate must be on or before today + 1 day");
        RuleFor(x => x.IdempotencyKey).NotEmpty().MaximumLength(80);
        RuleFor(x => x.Lines).NotNull().Must(l => l != null && l.Count >= 1 && l.Count <= 100)
            .WithMessage("lines must have between 1 and 100 items");
        When(x => x.Lines != null, () =>
        {
            RuleFor(x => x.Lines).Must(l => l!.Select(x => x.LineNumber).Distinct().Count() == l.Count)
                .WithMessage("lineNumber must be unique across lines");
            RuleForEach(x => x.Lines).SetValidator(new ClaimLineInputValidator());
        });
        When(x => x.Diagnoses != null, () =>
        {
            RuleForEach(x => x.Diagnoses!).SetValidator(new ClaimDiagnosisInputValidator());
        });
    }
}
