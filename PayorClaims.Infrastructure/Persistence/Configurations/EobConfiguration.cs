using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayorClaims.Domain.Entities;

namespace PayorClaims.Infrastructure.Persistence.Configurations;

public class EobConfiguration : IEntityTypeConfiguration<Eob>
{
    public void Configure(EntityTypeBuilder<Eob> b)
    {
        b.ToTable("eobs");
        b.Property(x => x.EobNumber).HasMaxLength(30).IsRequired();
        b.Property(x => x.DocumentStorageKey).HasMaxLength(500).IsRequired();
        b.Property(x => x.DocumentSha256).HasMaxLength(64).IsRequired();
        b.Property(x => x.DeliveryMethod).HasMaxLength(20).IsRequired();
        b.Property(x => x.DeliveryStatus).HasMaxLength(20).IsRequired();
        b.Property(x => x.FailureReason).HasMaxLength(500);

        b.HasOne(x => x.Claim).WithMany(c => c.Eobs).HasForeignKey(x => x.ClaimId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.EobNumber).IsUnique();
        b.HasIndex(x => x.ClaimId);
    }
}
