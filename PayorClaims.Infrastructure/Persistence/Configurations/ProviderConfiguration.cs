using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayorClaims.Domain.Entities;

namespace PayorClaims.Infrastructure.Persistence.Configurations;

public class ProviderConfiguration : IEntityTypeConfiguration<Provider>
{
    public void Configure(EntityTypeBuilder<Provider> b)
    {
        b.ToTable("providers");
        b.Property(x => x.Npi).HasMaxLength(10).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.ProviderType).HasMaxLength(30).IsRequired();
        b.Property(x => x.Specialty).HasMaxLength(100);
        b.Property(x => x.TaxId).HasMaxLength(20);
        b.Property(x => x.ProviderStatus).HasMaxLength(20).IsRequired();
        b.Property(x => x.TerminationReason).HasMaxLength(200);
    }
}
