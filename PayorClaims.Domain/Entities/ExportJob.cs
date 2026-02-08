namespace PayorClaims.Domain.Entities;

public class ExportJob
{
    public Guid Id { get; set; }
    public string RequestedByActorType { get; set; } = null!;
    public Guid? RequestedByActorId { get; set; }
    public Guid MemberId { get; set; }
    public string Status { get; set; } = "Queued"; // Queued, Running, Ready, Failed
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? FilePath { get; set; }
    public string? DownloadTokenHash { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
