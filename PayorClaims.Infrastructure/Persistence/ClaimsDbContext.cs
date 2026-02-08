using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PayorClaims.Domain.Common;
using PayorClaims.Domain.Entities;

namespace PayorClaims.Infrastructure.Persistence;

public class ClaimsDbContext : DbContext
{
    public ClaimsDbContext(DbContextOptions<ClaimsDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// When true, use IgnoreQueryFilters() on queries to include soft-deleted rows.
    /// </summary>
    public bool IncludeSoftDeleted { get; set; }

    // Core
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Coverage> Coverages => Set<Coverage>();
    public DbSet<Provider> Providers => Set<Provider>();
    public DbSet<ProviderLocation> ProviderLocations => Set<ProviderLocation>();
    // Claims
    public DbSet<Claim> Claims => Set<Claim>();
    public DbSet<ClaimLine> ClaimLines => Set<ClaimLine>();
    public DbSet<ClaimDiagnosis> ClaimDiagnoses => Set<ClaimDiagnosis>();
    // Payments
    public DbSet<Payment> Payments => Set<Payment>();
    // Benefits & Accumulators
    public DbSet<PlanBenefit> PlanBenefits => Set<PlanBenefit>();
    public DbSet<Accumulator> Accumulators => Set<Accumulator>();
    // Prior Auth
    public DbSet<PriorAuth> PriorAuths => Set<PriorAuth>();
    // COB
    public DbSet<MemberInsurancePolicy> MemberInsurancePolicies => Set<MemberInsurancePolicy>();
    // Appeals
    public DbSet<ClaimAppeal> ClaimAppeals => Set<ClaimAppeal>();
    // Attachments
    public DbSet<ClaimAttachment> ClaimAttachments => Set<ClaimAttachment>();
    // EOB
    public DbSet<Eob> Eobs => Set<Eob>();
    // Audit
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();
    // Consents
    public DbSet<MemberConsent> MemberConsents => Set<MemberConsent>();
    // HIPAA (append-only, no BaseEntity)
    public DbSet<HipaaAccessLog> HipaaAccessLogs => Set<HipaaAccessLog>();
    // Reference tables
    public DbSet<CptCode> CptCodes => Set<CptCode>();
    public DbSet<DiagnosisCode> DiagnosisCodes => Set<DiagnosisCode>();
    public DbSet<AdjustmentReasonCode> AdjustmentReasonCodes => Set<AdjustmentReasonCode>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClaimsDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(BaseEntity.RowVersion))
                    .IsRowVersion()
                    .IsConcurrencyToken();

                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var isDeletedProperty = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                var filter = Expression.Lambda(Expression.Not(isDeletedProperty), parameter);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }
    }

    public override int SaveChanges()
    {
        EnforceHipaaAccessLogAppendOnly();
        ApplyBaseEntityConventions();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        EnforceHipaaAccessLogAppendOnly();
        ApplyBaseEntityConventions();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void EnforceHipaaAccessLogAppendOnly()
    {
        foreach (var entry in ChangeTracker.Entries<HipaaAccessLog>())
        {
            if (entry.State is EntityState.Modified or EntityState.Deleted)
                throw new InvalidOperationException("HipaaAccessLog is append-only; modifications and deletions are not allowed.");
        }
    }

    private void ApplyBaseEntityConventions()
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is not BaseEntity entity)
                continue;

            switch (entry.State)
            {
                case EntityState.Added:
                    entity.CreatedAt = utcNow;
                    entity.UpdatedAt = utcNow;
                    entity.IsDeleted = false;
                    entity.DeletedAt = null;
                    break;

                case EntityState.Modified:
                    entity.UpdatedAt = utcNow;
                    entry.Property(nameof(BaseEntity.CreatedAt)).IsModified = false;
                    break;

                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entity.IsDeleted = true;
                    entity.DeletedAt = utcNow;
                    entity.UpdatedAt = utcNow;
                    break;
            }
        }
    }
}
