using PayorClaims.Domain.Common;

namespace PayorClaims.Domain.Entities;

public class ClaimAttachment : BaseEntity
{
    public Guid ClaimId { get; set; }
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public string StorageProvider { get; set; } = null!;
    public string StorageKey { get; set; } = null!;
    public string Sha256 { get; set; } = null!;
    public string UploadedByActorType { get; set; } = null!;
    public Guid? UploadedByActorId { get; set; }
    public DateTime UploadedAt { get; set; }

    public Claim Claim { get; set; } = null!;
}
