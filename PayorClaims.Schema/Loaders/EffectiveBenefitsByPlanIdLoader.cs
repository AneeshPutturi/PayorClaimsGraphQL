using GraphQL.DataLoader;
using Microsoft.EntityFrameworkCore;
using PayorClaims.Domain.Entities;
using PayorClaims.Infrastructure.Persistence;

namespace PayorClaims.Schema.Loaders;

/// <summary>Scoped loader: inject ClaimsDbContext. Batches by PlanId; filter by date in resolver (EffectiveFrom &lt;= date AND (EffectiveTo IS NULL OR EffectiveTo >= date)).</summary>
public class EffectiveBenefitsByPlanIdLoader
{
    public const string Key = "effectiveBenefitsByPlanId";
    private readonly ClaimsDbContext _db;

    public EffectiveBenefitsByPlanIdLoader(ClaimsDbContext db) => _db = db;

    public IDataLoaderResult<IEnumerable<PlanBenefit>?> LoadAsync(Guid planId, DataLoaderContext context) =>
        context.GetOrAddBatchLoader<Guid, IEnumerable<PlanBenefit>?>(Key, CreateBatch()).LoadAsync(planId);

    private Func<IEnumerable<Guid>, CancellationToken, Task<IDictionary<Guid, IEnumerable<PlanBenefit>?>>> CreateBatch() =>
        async (planIds, ct) =>
        {
            var list = await _db.PlanBenefits.AsNoTracking().Where(b => planIds.Contains(b.PlanId)).ToListAsync(ct);
            var lookup = list.ToLookup(b => b.PlanId);
            return planIds.ToDictionary(id => id, id => (IEnumerable<PlanBenefit>?)lookup[id]);
        };
}
