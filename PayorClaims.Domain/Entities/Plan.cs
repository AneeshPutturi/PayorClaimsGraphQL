using PayorClaims.Domain.Common;

namespace PayorClaims.Domain.Entities;

public class Plan : BaseEntity
{
    public string PlanCode { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int Year { get; set; }
    public string NetworkType { get; set; } = null!;
    public string MetalTier { get; set; } = null!;
    public bool IsActive { get; set; }
}
