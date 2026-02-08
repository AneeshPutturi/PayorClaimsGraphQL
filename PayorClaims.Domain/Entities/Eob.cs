using PayorClaims.Domain.Common;

namespace PayorClaims.Domain.Entities;

public class Eob : BaseEntity
{
    public Guid ClaimId { get; set; }
    public string EobNumber { get; set; } = null!;
    public DateTime GeneratedAt { get; set; }
    public string DocumentStorageKey { get; set; } = null!;
    public string DocumentSha256 { get; set; } = null!;
    public string DeliveryMethod { get; set; } = null!;
    public string DeliveryStatus { get; set; } = null!;
    public DateTime? DeliveredAt { get; set; }
    public string? FailureReason { get; set; }

    public Claim Claim { get; set; } = null!;
}
