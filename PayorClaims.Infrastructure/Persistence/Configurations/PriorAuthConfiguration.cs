using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayorClaims.Domain.Entities;

namespace PayorClaims.Infrastructure.Persistence.Configurations;

public class PriorAuthConfiguration : IEntityTypeConfiguration<PriorAuth>
{
    public void Configure(EntityTypeBuilder<PriorAuth> builder)
    {
        builder.ToTable("prior_auths");
        builder.Property(e => e.ServiceType).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Status).HasMaxLength(20).IsRequired();
        builder.Property(e => e.Notes).HasMaxLength(500);
        builder.HasIndex(e => new { e.MemberId, e.Status });
    }
}
