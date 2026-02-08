using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayorClaims.Domain.Entities;

namespace PayorClaims.Infrastructure.Persistence.Configurations;

public class ClaimAppealConfiguration : IEntityTypeConfiguration<ClaimAppeal>
{
    public void Configure(EntityTypeBuilder<ClaimAppeal> builder)
    {
        builder.ToTable("claim_appeals");
        builder.Property(e => e.Reason).HasMaxLength(500).IsRequired();
        builder.Property(e => e.Status).HasMaxLength(20).IsRequired();
        builder.HasOne(e => e.Claim).WithMany(c => c.Appeals).HasForeignKey(e => e.ClaimId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(e => e.ClaimId);
    }
}
