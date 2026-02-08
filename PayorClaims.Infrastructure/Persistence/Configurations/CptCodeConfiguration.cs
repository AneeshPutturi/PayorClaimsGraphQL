using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayorClaims.Domain.Entities;

namespace PayorClaims.Infrastructure.Persistence.Configurations;

public class CptCodeConfiguration : IEntityTypeConfiguration<CptCode>
{
    public void Configure(EntityTypeBuilder<CptCode> builder)
    {
        builder.ToTable("cpt_codes");
        builder.HasKey(x => x.CptCodeId);
        builder.Property(x => x.CptCodeId).HasMaxLength(10);
        builder.Property(x => x.Description).HasMaxLength(500).IsRequired();
    }
}
