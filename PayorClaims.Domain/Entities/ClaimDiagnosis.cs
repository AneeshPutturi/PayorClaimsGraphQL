using PayorClaims.Domain.Common;

namespace PayorClaims.Domain.Entities;

public class ClaimDiagnosis : BaseEntity
{
    public Guid ClaimId { get; set; }
    public int? LineNumber { get; set; }
    public string CodeSystem { get; set; } = null!;
    public string Code { get; set; } = null!;
    public bool IsPrimary { get; set; }

    public Claim Claim { get; set; } = null!;
}
