using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayorClaims.Domain.Entities;

namespace PayorClaims.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> b)
    {
        b.ToTable("payments");
        b.Property(x => x.Amount).HasPrecision(18, 2);
        b.Property(x => x.Method).HasMaxLength(20).IsRequired();
        b.Property(x => x.ReferenceNumber).HasMaxLength(50);
        b.Property(x => x.IdempotencyKey).HasMaxLength(80);

        b.HasOne(x => x.Claim).WithMany(c => c.Payments).HasForeignKey(x => x.ClaimId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.ClaimId, x.PaymentDate });
        b.HasIndex(x => x.IdempotencyKey).IsUnique().HasFilter("[IdempotencyKey] IS NOT NULL");
    }
}
