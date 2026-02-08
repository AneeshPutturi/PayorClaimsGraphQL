using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayorClaims.Domain.Entities;

namespace PayorClaims.Infrastructure.Persistence.Configurations;

public class ProviderLocationConfiguration : IEntityTypeConfiguration<ProviderLocation>
{
    public void Configure(EntityTypeBuilder<ProviderLocation> builder)
    {
        builder.ToTable("provider_locations");
        builder.Property(x => x.AddressLine1).HasMaxLength(200).IsRequired();
        builder.Property(x => x.AddressLine2).HasMaxLength(200);
        builder.Property(x => x.City).HasMaxLength(100).IsRequired();
        builder.Property(x => x.State).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Zip).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Phone).HasMaxLength(30);

        builder.HasOne(x => x.Provider).WithMany(p => p.Locations).HasForeignKey(x => x.ProviderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.ProviderId);
    }
}
