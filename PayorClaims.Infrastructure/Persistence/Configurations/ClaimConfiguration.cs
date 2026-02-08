using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayorClaims.Domain.Entities;

namespace PayorClaims.Infrastructure.Persistence.Configurations;

public class ClaimConfiguration : IEntityTypeConfiguration<Claim>
{
    public void Configure(EntityTypeBuilder<Claim> b)
    {
        b.ToTable("claims");
        b.Property(x => x.ClaimNumber).HasMaxLength(30).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.TotalBilled).HasPrecision(18, 2);
        b.Property(x => x.TotalAllowed).HasPrecision(18, 2);
        b.Property(x => x.TotalPaid).HasPrecision(18, 2);
        b.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        b.Property(x => x.IdempotencyKey).HasMaxLength(80);
        b.Property(x => x.DuplicateFingerprint).HasMaxLength(64).IsRequired();
        b.Property(x => x.SourceSystem).HasMaxLength(50);
        b.Property(x => x.SubmittedByActorType).HasMaxLength(20).IsRequired();

        b.HasOne(x => x.Member).WithMany().HasForeignKey(x => x.MemberId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Provider).WithMany().HasForeignKey(x => x.ProviderId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Coverage).WithMany().HasForeignKey(x => x.CoverageId).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.ClaimNumber).IsUnique();
        b.HasIndex(x => x.IdempotencyKey).IsUnique().HasFilter("[IdempotencyKey] IS NOT NULL");
        b.HasIndex(x => x.Status);
    }
}
