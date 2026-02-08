namespace PayorClaims.Domain.Entities;

/// <summary>
/// Reference table: natural key CptCodeId. Does NOT inherit BaseEntity.
/// </summary>
public class CptCode
{
    public string CptCodeId { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public bool IsActive { get; set; }
}
