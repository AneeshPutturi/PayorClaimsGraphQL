using PayorClaims.Domain.Common;

namespace PayorClaims.Domain.Entities;

public class PriorAuth : BaseEntity
{
    public Guid MemberId { get; set; }
    public Guid? ProviderId { get; set; }
    public string ServiceType { get; set; } = null!;
    public DateTime RequestedDate { get; set; }
    public DateTime? DecisionDate { get; set; }
    public string Status { get; set; } = null!;
    public string? Notes { get; set; }
}
