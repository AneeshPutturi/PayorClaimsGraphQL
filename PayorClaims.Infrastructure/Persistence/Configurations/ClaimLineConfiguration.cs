using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayorClaims.Domain.Entities;

namespace PayorClaims.Infrastructure.Persistence.Configurations;

public class ClaimLineConfiguration : IEntityTypeConfiguration<ClaimLine>
{
    public void Configure(EntityTypeBuilder<ClaimLine> b)
    {
        b.ToTable("claim_lines");
        b.Property(x => x.CptCode).HasMaxLength(10).IsRequired();
        b.Property(x => x.BilledAmount).HasPrecision(18, 2);
        b.Property(x => x.AllowedAmount).HasPrecision(18, 2);
        b.Property(x => x.PaidAmount).HasPrecision(18, 2);
        b.Property(x => x.LineStatus).HasMaxLength(30).IsRequired();
        b.Property(x => x.DenialReasonCode).HasMaxLength(20);
        b.Property(x => x.DenialReasonText).HasMaxLength(250);

        b.HasOne(x => x.Claim).WithMany(c => c.Lines).HasForeignKey(x => x.ClaimId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.DenialReason).WithMany(a => a.ClaimLinesWithDenial).HasForeignKey(x => x.DenialReasonCode).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.ClaimId);
        b.HasIndex(x => new { x.ClaimId, x.LineNumber }).IsUnique();
    }
}
