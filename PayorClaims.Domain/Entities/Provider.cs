using PayorClaims.Domain.Common;

namespace PayorClaims.Domain.Entities;

public class Provider : BaseEntity
{
    public string Npi { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string ProviderType { get; set; } = null!;
    public string? Specialty { get; set; }
    public string? TaxId { get; set; }
    public string ProviderStatus { get; set; } = null!;
    public DateOnly? CredentialedFrom { get; set; }
    public DateOnly? CredentialedTo { get; set; }
    public string? TerminationReason { get; set; }

    public ICollection<ProviderLocation> Locations { get; set; } = new List<ProviderLocation>();
}
