using PayorClaims.Domain.Common;

namespace PayorClaims.Domain.Entities;

public class ProviderLocation : BaseEntity
{
    public Guid ProviderId { get; set; }
    public string AddressLine1 { get; set; } = null!;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = null!;
    public string State { get; set; } = null!;
    public string Zip { get; set; } = null!;
    public string? Phone { get; set; }
    public bool IsPrimary { get; set; }

    public Provider Provider { get; set; } = null!;
}
