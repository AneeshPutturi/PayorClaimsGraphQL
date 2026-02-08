using PayorClaims.Domain.Common;

namespace PayorClaims.Domain.Entities;

public class ClaimLine : BaseEntity
{
    public Guid ClaimId { get; set; }
    public int LineNumber { get; set; }
    public string CptCode { get; set; } = null!;
    public int Units { get; set; }
    public decimal BilledAmount { get; set; }
    public decimal AllowedAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public string LineStatus { get; set; } = null!;
    public string? DenialReasonCode { get; set; }
    public string? DenialReasonText { get; set; }

    public Claim Claim { get; set; } = null!;
    public AdjustmentReasonCode? DenialReason { get; set; }
}
