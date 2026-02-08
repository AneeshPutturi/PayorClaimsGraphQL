using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayorClaims.Domain.Entities;

namespace PayorClaims.Infrastructure.Persistence.Configurations;

public class ClaimAttachmentConfiguration : IEntityTypeConfiguration<ClaimAttachment>
{
    public void Configure(EntityTypeBuilder<ClaimAttachment> b)
    {
        b.ToTable("claim_attachments");
        b.Property(x => x.FileName).HasMaxLength(200).IsRequired();
        b.Property(x => x.ContentType).HasMaxLength(100).IsRequired();
        b.Property(x => x.StorageProvider).HasMaxLength(20).IsRequired();
        b.Property(x => x.StorageKey).HasMaxLength(500).IsRequired();
        b.Property(x => x.Sha256).HasMaxLength(64).IsRequired();
        b.Property(x => x.UploadedByActorType).HasMaxLength(20).IsRequired();

        b.HasOne(x => x.Claim).WithMany(c => c.Attachments).HasForeignKey(x => x.ClaimId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.ClaimId);
        b.HasIndex(x => x.Sha256);
    }
}
