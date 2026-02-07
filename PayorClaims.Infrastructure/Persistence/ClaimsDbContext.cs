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
    /// The global filter excludes IsDeleted == true by default.
    /// </summary>
    public bool IncludeSoftDeleted { get; set; }

    public DbSet<TestEntity> TestEntities => Set<TestEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                // RowVersion + concurrency token
                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(BaseEntity.RowVersion))
                    .IsRowVersion()
                    .IsConcurrencyToken();

                // Global query filter: exclude soft-deleted by default (use IgnoreQueryFilters() when IncludeSoftDeleted is true)
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var isDeletedProperty = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                var filter = Expression.Lambda(Expression.Not(isDeletedProperty), parameter);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }
    }

    public override int SaveChanges()
    {
        ApplyBaseEntityConventions();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyBaseEntityConventions();
        return base.SaveChangesAsync(cancellationToken);
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
