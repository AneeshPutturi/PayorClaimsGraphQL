using GraphQL;
using GraphQL.DataLoader;
using Microsoft.EntityFrameworkCore;
using PayorClaims.Domain.Entities;
using PayorClaims.Infrastructure.Persistence;

namespace PayorClaims.Schema.Loaders;

/// <summary>Scoped loader: inject ClaimsDbContext. Resolve from context.RequestServices in resolvers.</summary>
public class ClaimLinesByClaimIdLoader
{
    public const string Key = "claimLinesByClaimId";
    private readonly ClaimsDbContext _db;

    public ClaimLinesByClaimIdLoader(ClaimsDbContext db) => _db = db;

    public IDataLoaderResult<IEnumerable<ClaimLine>?> LoadAsync(Guid claimId, DataLoaderContext context) =>
        context.GetOrAddBatchLoader<Guid, IEnumerable<ClaimLine>?>(Key, CreateBatch()).LoadAsync(claimId);

    private Func<IEnumerable<Guid>, CancellationToken, Task<IDictionary<Guid, IEnumerable<ClaimLine>?>>> CreateBatch() =>
        async (claimIds, ct) =>
        {
            var list = await _db.ClaimLines.AsNoTracking().Where(l => claimIds.Contains(l.ClaimId)).ToListAsync(ct);
            var lookup = list.ToLookup(l => l.ClaimId);
            return claimIds.ToDictionary(id => id, id => (IEnumerable<ClaimLine>?)lookup[id]);
        };
}
