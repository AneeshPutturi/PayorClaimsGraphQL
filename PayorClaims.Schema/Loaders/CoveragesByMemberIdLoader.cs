using GraphQL;
using GraphQL.DataLoader;
using Microsoft.EntityFrameworkCore;
using PayorClaims.Domain.Entities;
using PayorClaims.Infrastructure.Persistence;

namespace PayorClaims.Schema.Loaders;

/// <summary>Scoped loader: inject ClaimsDbContext. Resolve from context.RequestServices in resolvers.</summary>
public class CoveragesByMemberIdLoader
{
    public const string Key = "coveragesByMemberId";
    private readonly ClaimsDbContext _db;

    public CoveragesByMemberIdLoader(ClaimsDbContext db) => _db = db;

    public IDataLoaderResult<IEnumerable<Coverage>?> LoadAsync(Guid memberId, DataLoaderContext context) =>
        context.GetOrAddBatchLoader<Guid, IEnumerable<Coverage>?>(Key, CreateBatch()).LoadAsync(memberId);

    private Func<IEnumerable<Guid>, CancellationToken, Task<IDictionary<Guid, IEnumerable<Coverage>?>>> CreateBatch() =>
        async (memberIds, ct) =>
        {
            var list = await _db.Coverages.AsNoTracking().Where(c => memberIds.Contains(c.MemberId)).ToListAsync(ct);
            var lookup = list.ToLookup(c => c.MemberId);
            return memberIds.ToDictionary(id => id, id => (IEnumerable<Coverage>?)lookup[id]);
        };
}
