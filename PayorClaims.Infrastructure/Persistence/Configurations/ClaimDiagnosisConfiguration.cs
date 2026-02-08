using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayorClaims.Domain.Entities;

namespace PayorClaims.Infrastructure.Persistence.Configurations;

public class ClaimDiagnosisConfiguration : IEntityTypeConfiguration<ClaimDiagnosis>
{
    public void Configure(EntityTypeBuilder<ClaimDiagnosis> b)
    {
        b.ToTable("claim_diagnoses");
        b.Property(x => x.CodeSystem).HasMaxLength(20).IsRequired();
        b.Property(x => x.Code).HasMaxLength(20).IsRequired();

        b.HasOne(x => x.Claim).WithMany(c => c.Diagnoses).HasForeignKey(x => x.ClaimId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.ClaimId, x.LineNumber });
    }
}
