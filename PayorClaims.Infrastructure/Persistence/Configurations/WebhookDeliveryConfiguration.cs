using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayorClaims.Domain.Entities;

namespace PayorClaims.Infrastructure.Persistence.Configurations;

public class WebhookDeliveryConfiguration : IEntityTypeConfiguration<WebhookDelivery>
{
    public void Configure(EntityTypeBuilder<WebhookDelivery> b)
    {
        b.ToTable("webhook_deliveries");
        b.HasKey(x => x.Id);
        b.Property(x => x.EventType).HasMaxLength(100).IsRequired();
        b.Property(x => x.PayloadJson).IsRequired();
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.Property(x => x.LastError).HasMaxLength(2000);
        b.HasIndex(x => new { x.Status, x.NextAttemptAt });
    }
}
