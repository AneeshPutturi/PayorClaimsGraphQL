using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayorClaims.Domain.Entities;

namespace PayorClaims.Infrastructure.Persistence.Configurations;

public class HipaaAccessLogConfiguration : IEntityTypeConfiguration<HipaaAccessLog>
{
    public void Configure(EntityTypeBuilder<HipaaAccessLog> builder)
    {
        builder.ToTable("hipaa_access_logs");
        builder.HasKey(x => x.AccessLogId);
        builder.Property(x => x.ActorType).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Action).HasMaxLength(30).IsRequired();
        builder.Property(x => x.SubjectType).HasMaxLength(30).IsRequired();
        builder.Property(x => x.IpAddress).HasMaxLength(50);
        builder.Property(x => x.UserAgent).HasMaxLength(300);
        builder.Property(x => x.PurposeOfUse).HasMaxLength(50).IsRequired();
        builder.Property(x => x.PrevHash).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Hash).HasMaxLength(64).IsRequired();

        builder.HasIndex(x => x.OccurredAt);
        builder.HasIndex(x => new { x.SubjectType, x.SubjectId });
        builder.HasIndex(x => new { x.ActorType, x.ActorId });
    }
}
