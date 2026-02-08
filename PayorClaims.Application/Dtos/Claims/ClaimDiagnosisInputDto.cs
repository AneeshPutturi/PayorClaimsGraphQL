namespace PayorClaims.Application.Dtos.Claims;

public class ClaimDiagnosisInputDto
{
    public string CodeSystem { get; set; } = null!;
    public string Code { get; set; } = null!;
    public bool IsPrimary { get; set; }
    public int? LineNumber { get; set; }
}
