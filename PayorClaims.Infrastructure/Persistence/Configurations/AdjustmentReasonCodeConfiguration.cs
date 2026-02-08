using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayorClaims.Domain.Entities;

namespace PayorClaims.Infrastructure.Persistence.Configurations;

public class AdjustmentReasonCodeConfiguration : IEntityTypeConfiguration<AdjustmentReasonCode>
{
    public void Configure(EntityTypeBuilder<AdjustmentReasonCode> builder)
    {
        builder.ToTable("adjustment_reason_codes");
        builder.HasKey(x => x.Code);
        builder.Property(x => x.Code).HasMaxLength(20);
        builder.Property(x => x.CodeType).HasMaxLength(10).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500).IsRequired();
    }
}
