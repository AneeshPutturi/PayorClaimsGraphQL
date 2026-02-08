using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayorClaims.Domain.Entities;

namespace PayorClaims.Infrastructure.Persistence.Configurations;

public class DiagnosisCodeConfiguration : IEntityTypeConfiguration<DiagnosisCode>
{
    public void Configure(EntityTypeBuilder<DiagnosisCode> builder)
    {
        builder.ToTable("diagnosis_codes");
        builder.HasKey(x => new { x.CodeSystem, x.Code });
        builder.Property(x => x.CodeSystem).HasMaxLength(20);
        builder.Property(x => x.Code).HasMaxLength(20);
        builder.Property(x => x.Description).HasMaxLength(500).IsRequired();
    }
}
