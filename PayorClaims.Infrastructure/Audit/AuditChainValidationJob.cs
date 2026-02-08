using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PayorClaims.Domain.Entities;
using PayorClaims.Infrastructure.Persistence;

namespace PayorClaims.Infrastructure.Audit;

/// <summary>
/// Runs every 1 hour to verify AuditEvent hash chain integrity.
/// </summary>
public class AuditChainValidationJob : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);
    private const int MaxRecordsToValidate = 10_000;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AuditChainValidationJob> _logger;

    public AuditChainValidationJob(IServiceScopeFactory scopeFactory, ILogger<AuditChainValidationJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ValidateChainAsync(stoppingToken);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Audit chain validation failed");
            }
            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task ValidateChainAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ClaimsDbContext>();
        var events = await db.AuditEvents.IgnoreQueryFilters()
            .OrderBy(e => e.OccurredAt).ThenBy(e => e.Id)
            .Take(MaxRecordsToValidate)
            .Select(e => new { e.Id, e.PrevHash, e.Hash, e.ActorUserId, e.Action, e.EntityType, e.EntityId, e.OccurredAt, e.DiffJson })
            .ToListAsync(ct);

        if (events.Count == 0) return;

        string? prevHash = null;
        foreach (var e in events)
        {
            var expectedPrev = prevHash ?? "";
            if (e.PrevHash != expectedPrev)
            {
                _logger.LogCritical("Audit chain integrity broken at event {EventId}: PrevHash mismatch (expected chain continuity)", e.Id);
                return;
            }
            var payload = $"{e.PrevHash}|{e.ActorUserId}|{e.Action}|{e.EntityType}|{e.EntityId}|{e.OccurredAt:O}|{e.DiffJson ?? ""}";
            var expectedHash = ToHex(SHA256.HashData(Encoding.UTF8.GetBytes(payload)));
            if (e.Hash != expectedHash)
            {
                _logger.LogCritical("Audit chain integrity broken at event {EventId}: Hash mismatch (tampering detected)", e.Id);
                return;
            }
            prevHash = e.Hash;
        }
        _logger.LogDebug("Audit chain validation passed for {Count} events", events.Count);
    }

    private static string ToHex(byte[] bytes) => Convert.ToHexString(bytes).ToLowerInvariant();
}
