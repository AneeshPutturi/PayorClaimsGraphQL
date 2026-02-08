namespace PayorClaims.Domain.Entities;

/// <summary>
/// Reference table: natural key Code. Does NOT inherit BaseEntity.
/// </summary>
public class AdjustmentReasonCode
{
    public string Code { get; set; } = null!;
    public string CodeType { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsActive { get; set; }

    public ICollection<ClaimLine> ClaimLinesWithDenial { get; set; } = new List<ClaimLine>();
}
