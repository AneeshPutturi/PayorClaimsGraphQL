namespace PayorClaims.Domain.Entities;

/// <summary>
/// Append-only HIPAA access log. Does NOT inherit BaseEntity (no soft delete / no updates).
/// </summary>
public class HipaaAccessLog
{
    public Guid AccessLogId { get; set; }
    public string ActorType { get; set; } = null!;
    public Guid? ActorId { get; set; }
    public string Action { get; set; } = null!;
    public string SubjectType { get; set; } = null!;
    public Guid SubjectId { get; set; }
    public DateTime OccurredAt { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string PurposeOfUse { get; set; } = null!;
    public string PrevHash { get; set; } = null!;
    public string Hash { get; set; } = null!;
}
