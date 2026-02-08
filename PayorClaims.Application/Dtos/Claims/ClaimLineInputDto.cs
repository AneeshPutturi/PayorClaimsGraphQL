namespace PayorClaims.Application.Dtos.Claims;

public class ClaimLineInputDto
{
    public int LineNumber { get; set; }
    public string CptCode { get; set; } = null!;
    public int Units { get; set; }
    public decimal BilledAmount { get; set; }
    public List<ClaimDiagnosisInputDto>? Diagnoses { get; set; }
}
