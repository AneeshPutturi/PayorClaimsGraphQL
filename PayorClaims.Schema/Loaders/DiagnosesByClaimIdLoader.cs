using GraphQL.DataLoader;
using Microsoft.EntityFrameworkCore;
using PayorClaims.Domain.Entities;
using PayorClaims.Infrastructure.Persistence;

namespace PayorClaims.Schema.Loaders;

/// <summary>Scoped loader: inject ClaimsDbContext. Resolve from context.RequestServices in resolvers.</summary>
public class DiagnosesByClaimIdLoader
{
    public const string Key = "diagnosesByClaimId";
    private readonly ClaimsDbContext _db;

    public DiagnosesByClaimIdLoader(ClaimsDbContext db) => _db = db;

    public IDataLoaderResult<IEnumerable<ClaimDiagnosis>?> LoadAsync(Guid claimId, DataLoaderContext context) =>
        context.GetOrAddBatchLoader<Guid, IEnumerable<ClaimDiagnosis>?>(Key, CreateBatch()).LoadAsync(claimId);

    private Func<IEnumerable<Guid>, CancellationToken, Task<IDictionary<Guid, IEnumerable<ClaimDiagnosis>?>>> CreateBatch() =>
        async (claimIds, ct) =>
        {
            var list = await _db.ClaimDiagnoses.AsNoTracking().Where(d => claimIds.Contains(d.ClaimId)).ToListAsync(ct);
            var lookup = list.ToLookup(d => d.ClaimId);
            return claimIds.ToDictionary(id => id, id => (IEnumerable<ClaimDiagnosis>?)lookup[id]);
        };
}
