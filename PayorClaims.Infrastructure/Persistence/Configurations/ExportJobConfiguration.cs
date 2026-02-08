using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayorClaims.Domain.Entities;

namespace PayorClaims.Infrastructure.Persistence.Configurations;

public class ExportJobConfiguration : IEntityTypeConfiguration<ExportJob>
{
    public void Configure(EntityTypeBuilder<ExportJob> b)
    {
        b.ToTable("export_jobs");
        b.HasKey(x => x.Id);
        b.Property(x => x.RequestedByActorType).HasMaxLength(50).IsRequired();
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.Property(x => x.FilePath).HasMaxLength(1000);
        b.Property(x => x.DownloadTokenHash).HasMaxLength(64);
        b.HasIndex(x => new { x.MemberId, x.Status });
        b.HasIndex(x => x.CreatedAt);
    }
}
