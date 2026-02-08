using PayorClaims.Domain.Common;

namespace PayorClaims.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid ClaimId { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = null!;
    public string? ReferenceNumber { get; set; }
    public string? IdempotencyKey { get; set; }

    public Claim Claim { get; set; } = null!;
}
