using PayorClaims.Domain.Common;

namespace PayorClaims.Domain.Entities;

public class PlanBenefit : BaseEntity
{
    public Guid PlanId { get; set; }
    public int BenefitVersion { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public string Category { get; set; } = null!;
    public string Network { get; set; } = null!;
    public string CoverageLevel { get; set; } = null!;
    public string Period { get; set; } = null!;
    public decimal? CopayAmount { get; set; }
    public decimal? CoinsurancePercent { get; set; }
    public bool DeductibleApplies { get; set; }
    public int? MaxVisits { get; set; }
    public string? Notes { get; set; }

    public Plan Plan { get; set; } = null!;
}
