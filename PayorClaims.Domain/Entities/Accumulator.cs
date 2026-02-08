using PayorClaims.Domain.Common;

namespace PayorClaims.Domain.Entities;

public class Accumulator : BaseEntity
{
    public Guid MemberId { get; set; }
    public Guid PlanId { get; set; }
    public int Year { get; set; }
    public string Network { get; set; } = null!;
    public decimal DeductibleMet { get; set; }
    public decimal MoopMet { get; set; }
}
