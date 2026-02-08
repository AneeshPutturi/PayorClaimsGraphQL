using PayorClaims.Domain.Common;

namespace PayorClaims.Domain.Entities;

public class MemberConsent : BaseEntity
{
    public Guid MemberId { get; set; }
    public string ConsentType { get; set; } = null!;
    public bool Granted { get; set; }
    public DateTime GrantedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string Source { get; set; } = null!;
}
