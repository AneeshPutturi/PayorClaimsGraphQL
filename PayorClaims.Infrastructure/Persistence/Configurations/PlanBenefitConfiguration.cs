using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayorClaims.Domain.Entities;

namespace PayorClaims.Infrastructure.Persistence.Configurations;

public class PlanBenefitConfiguration : IEntityTypeConfiguration<PlanBenefit>
{
    public void Configure(EntityTypeBuilder<PlanBenefit> builder)
    {
        builder.ToTable("plan_benefits");
        builder.Property(x => x.Category).HasMaxLength(40).IsRequired();
        builder.Property(x => x.Network).HasMaxLength(20).IsRequired();
        builder.Property(x => x.CoverageLevel).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Period).HasMaxLength(20).IsRequired();
        builder.Property(x => x.CopayAmount).HasPrecision(18, 2);
        builder.Property(x => x.CoinsurancePercent).HasPrecision(5, 2);
        builder.Property(x => x.Notes).HasMaxLength(500);

        builder.HasOne(x => x.Plan).WithMany().HasForeignKey(x => x.PlanId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.PlanId, x.Category, x.Network });
    }
}
