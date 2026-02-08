using GraphQL.DataLoader;
using Microsoft.EntityFrameworkCore;
using PayorClaims.Domain.Entities;
using PayorClaims.Infrastructure.Persistence;

namespace PayorClaims.Schema.Loaders;

/// <summary>Scoped loader: inject ClaimsDbContext. Resolve from context.RequestServices in resolvers.</summary>
public class ClaimsByMemberIdLoader
{
    public const string Key = "claimsByMemberId";
    private readonly ClaimsDbContext _db;

    public ClaimsByMemberIdLoader(ClaimsDbContext db) => _db = db;

    public IDataLoaderResult<IEnumerable<Claim>?> LoadAsync(Guid memberId, DataLoaderContext context) =>
        context.GetOrAddBatchLoader<Guid, IEnumerable<Claim>?>(Key, CreateBatch()).LoadAsync(memberId);

    private Func<IEnumerable<Guid>, CancellationToken, Task<IDictionary<Guid, IEnumerable<Claim>?>>> CreateBatch() =>
        async (memberIds, ct) =>
        {
            var list = await _db.Claims.AsNoTracking().Where(c => memberIds.Contains(c.MemberId)).ToListAsync(ct);
            var lookup = list.ToLookup(c => c.MemberId);
            return memberIds.ToDictionary(id => id, id => (IEnumerable<Claim>?)lookup[id]);
        };
}
