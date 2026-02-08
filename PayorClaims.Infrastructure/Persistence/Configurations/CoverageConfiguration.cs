using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayorClaims.Domain.Entities;

namespace PayorClaims.Infrastructure.Persistence.Configurations;

public class CoverageConfiguration : IEntityTypeConfiguration<Coverage>
{
    public void Configure(EntityTypeBuilder<Coverage> b)
    {
        b.ToTable("coverages");
        b.Property(x => x.CoverageStatus).HasMaxLength(20).IsRequired();

        b.HasOne(x => x.Member).WithMany().HasForeignKey(x => x.MemberId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Plan).WithMany().HasForeignKey(x => x.PlanId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.MemberId, x.StartDate });
    }
}
