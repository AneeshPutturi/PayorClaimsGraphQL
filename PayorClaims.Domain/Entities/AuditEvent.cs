using PayorClaims.Domain.Common;

namespace PayorClaims.Domain.Entities;

public class AuditEvent : BaseEntity
{
    public string ActorUserId { get; set; } = null!;
    public string Action { get; set; } = null!;
    public string EntityType { get; set; } = null!;
    public Guid EntityId { get; set; }
    public DateTime OccurredAt { get; set; }
    public string? DiffJson { get; set; }
    public string? Notes { get; set; }
}
