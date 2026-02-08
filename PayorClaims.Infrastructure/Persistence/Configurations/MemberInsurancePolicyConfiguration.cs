using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayorClaims.Domain.Entities;

namespace PayorClaims.Infrastructure.Persistence.Configurations;

public class MemberInsurancePolicyConfiguration : IEntityTypeConfiguration<MemberInsurancePolicy>
{
    public void Configure(EntityTypeBuilder<MemberInsurancePolicy> builder)
    {
        builder.ToTable("member_insurance_policies");
        builder.Property(e => e.PayerName).HasMaxLength(200).IsRequired();
        builder.Property(e => e.PolicyNumber).HasMaxLength(50).IsRequired();
        builder.HasIndex(e => new { e.MemberId, e.Priority });
    }
}
