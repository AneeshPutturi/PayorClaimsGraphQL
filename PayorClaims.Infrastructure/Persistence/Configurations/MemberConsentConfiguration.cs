using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayorClaims.Domain.Entities;

namespace PayorClaims.Infrastructure.Persistence.Configurations;

public class MemberConsentConfiguration : IEntityTypeConfiguration<MemberConsent>
{
    public void Configure(EntityTypeBuilder<MemberConsent> b)
    {
        b.ToTable("member_consents");
        b.Property(x => x.ConsentType).HasMaxLength(50).IsRequired();
        b.Property(x => x.Source).HasMaxLength(50).IsRequired();

        b.HasIndex(x => new { x.MemberId, x.ConsentType });
    }
}
