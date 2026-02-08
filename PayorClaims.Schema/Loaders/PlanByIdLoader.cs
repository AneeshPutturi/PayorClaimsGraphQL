using GraphQL;
using GraphQL.DataLoader;
using Microsoft.EntityFrameworkCore;
using PayorClaims.Domain.Entities;
using PayorClaims.Infrastructure.Persistence;

namespace PayorClaims.Schema.Loaders;

/// <summary>Scoped loader: inject ClaimsDbContext. Resolve from context.RequestServices in resolvers.</summary>
public class PlanByIdLoader
{
    public const string Key = "planById";
    private readonly ClaimsDbContext _db;

    public PlanByIdLoader(ClaimsDbContext db) => _db = db;

    public IDataLoaderResult<Plan?> LoadAsync(Guid planId, DataLoaderContext context) =>
        context.GetOrAddBatchLoader<Guid, Plan?>(Key, CreateBatch()).LoadAsync(planId);

    private Func<IEnumerable<Guid>, CancellationToken, Task<IDictionary<Guid, Plan?>>> CreateBatch() =>
        async (planIds, ct) =>
        {
            var list = await _db.Plans.AsNoTracking().Where(p => planIds.Contains(p.Id)).ToListAsync(ct);
            var byId = list.ToDictionary(p => p.Id);
            return (IDictionary<Guid, Plan?>)planIds.ToDictionary(id => id, id => byId.TryGetValue(id, out var p) ? p : null);
        };
}
