using PayorClaims.Domain.Common;

namespace PayorClaims.Domain.Entities;

public class MemberInsurancePolicy : BaseEntity
{
    public Guid MemberId { get; set; }
    public string PayerName { get; set; } = null!;
    public string PolicyNumber { get; set; } = null!;
    public int Priority { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
}
