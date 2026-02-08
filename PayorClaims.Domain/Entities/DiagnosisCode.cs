namespace PayorClaims.Domain.Entities;

/// <summary>
/// Reference table: composite key (CodeSystem, Code). Does NOT inherit BaseEntity.
/// </summary>
public class DiagnosisCode
{
    public string CodeSystem { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public bool IsActive { get; set; }
}
