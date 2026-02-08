using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayorClaims.Domain.Entities;

namespace PayorClaims.Infrastructure.Persistence.Configurations;

public class AccumulatorConfiguration : IEntityTypeConfiguration<Accumulator>
{
    public void Configure(EntityTypeBuilder<Accumulator> builder)
    {
        builder.ToTable("accumulators");
        builder.Property(e => e.Network).HasMaxLength(20).IsRequired();
        builder.Property(e => e.DeductibleMet).HasPrecision(18, 2);
        builder.Property(e => e.MoopMet).HasPrecision(18, 2);
        builder.HasIndex(e => new { e.MemberId, e.PlanId, e.Year, e.Network }).IsUnique();
    }
}
