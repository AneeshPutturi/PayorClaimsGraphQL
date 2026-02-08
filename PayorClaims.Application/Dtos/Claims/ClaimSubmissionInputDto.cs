namespace PayorClaims.Application.Dtos.Claims;

public class ClaimSubmissionInputDto
{
    public Guid MemberId { get; set; }
    public Guid ProviderId { get; set; }
    public DateOnly ServiceFrom { get; set; }
    public DateOnly ServiceTo { get; set; }
    public DateOnly ReceivedDate { get; set; }
    public string IdempotencyKey { get; set; } = null!;
    public List<ClaimDiagnosisInputDto>? Diagnoses { get; set; }
    public List<ClaimLineInputDto> Lines { get; set; } = null!;
}
