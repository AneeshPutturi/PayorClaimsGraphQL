namespace PayorClaims.Application.Abstractions;

public interface IPersistedQueryStore
{
    string? GetQueryByHash(string sha256Hash);
    Task<string?> GetQueryByHashAsync(string sha256Hash, CancellationToken ct = default);
}
