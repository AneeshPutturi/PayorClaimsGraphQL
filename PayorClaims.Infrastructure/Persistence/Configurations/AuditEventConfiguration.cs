using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayorClaims.Domain.Entities;

namespace PayorClaims.Infrastructure.Persistence.Configurations;

public class AuditEventConfiguration : IEntityTypeConfiguration<AuditEvent>
{
    public void Configure(EntityTypeBuilder<AuditEvent> b)
    {
        b.ToTable("audit_events");
        b.Property(x => x.ActorUserId).HasMaxLength(100).IsRequired();
        b.Property(x => x.Action).HasMaxLength(50).IsRequired();
        b.Property(x => x.EntityType).HasMaxLength(50).IsRequired();
        b.Property(x => x.Notes).HasMaxLength(500);
        b.Property(x => x.PrevHash).HasMaxLength(64).IsRequired();
        b.Property(x => x.Hash).HasMaxLength(64).IsRequired();

        b.HasIndex(x => new { x.EntityType, x.EntityId });
        b.HasIndex(x => x.OccurredAt);
    }
}
