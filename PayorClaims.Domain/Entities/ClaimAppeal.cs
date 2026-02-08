using PayorClaims.Domain.Common;

namespace PayorClaims.Domain.Entities;

public class ClaimAppeal : BaseEntity
{
    public Guid ClaimId { get; set; }
    public int AppealLevel { get; set; }
    public string Reason { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime SubmittedAt { get; set; }
    public DateTime? DecisionAt { get; set; }

    public Claim Claim { get; set; } = null!;
}
