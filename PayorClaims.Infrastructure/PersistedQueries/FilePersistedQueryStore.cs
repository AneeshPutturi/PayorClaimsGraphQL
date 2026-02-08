using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PayorClaims.Application.Abstractions;

namespace PayorClaims.Infrastructure.PersistedQueries;

public class FilePersistedQueryStore : IPersistedQueryStore
{
    private readonly string _filePath;
    private readonly ILogger<FilePersistedQueryStore> _logger;
    private readonly Dictionary<string, string> _cache;

    public FilePersistedQueryStore(IOptions<FilePersistedQueryStoreOptions> options, ILogger<FilePersistedQueryStore> logger)
    {
        _filePath = options.Value.FilePath ?? "persisted-queries.json";
        _logger = logger;
        _cache = LoadOnce();
    }

    public string? GetQueryByHash(string sha256Hash)
    {
        return _cache.TryGetValue(sha256Hash.ToLowerInvariant(), out var q) ? q : null;
    }

    public Task<string?> GetQueryByHashAsync(string sha256Hash, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(GetQueryByHash(sha256Hash));
    }

    private Dictionary<string, string> LoadOnce()
    {
        try
        {
            if (!File.Exists(_filePath))
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var json = File.ReadAllText(_filePath);
            var raw = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
            var cache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in raw)
                cache[kv.Key] = kv.Value;
            return cache;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load persisted queries from {Path}", _filePath);
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }
}

public class FilePersistedQueryStoreOptions
{
    public string FilePath { get; set; } = "persisted-queries.json";
}
