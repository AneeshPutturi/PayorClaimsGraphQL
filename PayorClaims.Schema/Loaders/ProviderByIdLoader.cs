using GraphQL;
using GraphQL.DataLoader;
using Microsoft.EntityFrameworkCore;
using PayorClaims.Domain.Entities;
using PayorClaims.Infrastructure.Persistence;

namespace PayorClaims.Schema.Loaders;

/// <summary>Scoped loader: inject ClaimsDbContext. Resolve from context.RequestServices in resolvers.</summary>
public class ProviderByIdLoader
{
    public const string Key = "providerById";
    private readonly ClaimsDbContext _db;

    public ProviderByIdLoader(ClaimsDbContext db) => _db = db;

    public IDataLoaderResult<Provider?> LoadAsync(Guid providerId, DataLoaderContext context) =>
        context.GetOrAddBatchLoader<Guid, Provider?>(Key, CreateBatch()).LoadAsync(providerId);

    private Func<IEnumerable<Guid>, CancellationToken, Task<IDictionary<Guid, Provider?>>> CreateBatch() =>
        async (providerIds, ct) =>
        {
            var list = await _db.Providers.AsNoTracking().Where(p => providerIds.Contains(p.Id)).ToListAsync(ct);
            var byId = list.ToDictionary(p => p.Id);
            return (IDictionary<Guid, Provider?>)providerIds.ToDictionary(id => id, id => byId.TryGetValue(id, out var p) ? p : null);
        };
}
