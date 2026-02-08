using PayorClaims.Domain.Common;

namespace PayorClaims.Domain.Entities;

public class Coverage : BaseEntity
{
    public Guid MemberId { get; set; }
    public Guid PlanId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string CoverageStatus { get; set; } = null!;

    public Member Member { get; set; } = null!;
    public Plan Plan { get; set; } = null!;
}
