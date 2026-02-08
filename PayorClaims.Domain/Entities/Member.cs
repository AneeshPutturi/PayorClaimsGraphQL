using PayorClaims.Domain.Common;

namespace PayorClaims.Domain.Entities;

public class Member : BaseEntity
{
    public string ExternalMemberNumber { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateOnly Dob { get; set; }
    public string? Gender { get; set; }
    public string Status { get; set; } = null!;
    public byte[]? EmailEncrypted { get; set; }
    public byte[]? PhoneEncrypted { get; set; }
    public byte[]? SsnEncrypted { get; set; }
    /// <summary>Optional plain SSN when not using encryption (e.g. test/dev). When set, Admin/Adjuster/self Member may see it.</summary>
    public string? SsnPlain { get; set; }
}
