using PayorClaims.Domain.Common;

namespace PayorClaims.Domain.Entities;

public class Claim : BaseEntity
{
    public string ClaimNumber { get; set; } = null!;
    public Guid MemberId { get; set; }
    public Guid ProviderId { get; set; }
    public Guid? CoverageId { get; set; }
    public DateOnly ReceivedDate { get; set; }
    public DateOnly ServiceFromDate { get; set; }
    public DateOnly ServiceToDate { get; set; }
    public string Status { get; set; } = null!;
    public decimal TotalBilled { get; set; }
    public decimal TotalAllowed { get; set; }
    public decimal TotalPaid { get; set; }
    public string Currency { get; set; } = "USD";
    public string? IdempotencyKey { get; set; }
    public string DuplicateFingerprint { get; set; } = null!;
    public string? SourceSystem { get; set; }
    public string SubmittedByActorType { get; set; } = null!;
    public Guid? SubmittedByActorId { get; set; }

    public Member Member { get; set; } = null!;
    public Provider Provider { get; set; } = null!;
    public Coverage? Coverage { get; set; }
    public ICollection<ClaimLine> Lines { get; set; } = new List<ClaimLine>();
    public ICollection<ClaimDiagnosis> Diagnoses { get; set; } = new List<ClaimDiagnosis>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<ClaimAppeal> Appeals { get; set; } = new List<ClaimAppeal>();
    public ICollection<ClaimAttachment> Attachments { get; set; } = new List<ClaimAttachment>();
    public ICollection<Eob> Eobs { get; set; } = new List<Eob>();
}
