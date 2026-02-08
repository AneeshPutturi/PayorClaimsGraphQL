using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using PayorClaims.Application.Abstractions;
using PayorClaims.Domain.Entities;
using PayorClaims.Infrastructure.Persistence;

namespace PayorClaims.Infrastructure.Services;

public class AccessLogger : IAccessLogger
{
    private const string ActionRead = "Read";
    private const string ZeroHash = "0000000000000000000000000000000000000000000000000000000000000000";
    private readonly ClaimsDbContext _db;

    public AccessLogger(ClaimsDbContext db) => _db = db;

    public async Task LogReadAsync(string actorType, Guid? actorId, string subjectType, Guid subjectId, string purpose, CancellationToken ct = default)
    {
        var occurredAt = DateTime.UtcNow;
        var prevHash = await _db.HipaaAccessLogs.AsNoTracking()
            .Where(h => h.ActorType == actorType && h.ActorId == actorId)
            .OrderByDescending(h => h.OccurredAt)
            .Select(h => h.Hash)
            .FirstOrDefaultAsync(ct) ?? ZeroHash;

        var payload = $"{prevHash}|{actorType}|{actorId}|{ActionRead}|{subjectType}|{subjectId}|{occurredAt:o}|{purpose}";
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        var hash = Convert.ToHexString(hashBytes).ToLowerInvariant(); // 64 chars

        var log = new HipaaAccessLog
        {
            AccessLogId = Guid.NewGuid(),
            ActorType = actorType,
            ActorId = actorId,
            Action = ActionRead,
            SubjectType = subjectType,
            SubjectId = subjectId,
            OccurredAt = occurredAt,
            PurposeOfUse = purpose,
            PrevHash = prevHash,
            Hash = hash
        };
        _db.HipaaAccessLogs.Add(log);
        await _db.SaveChangesAsync(ct);
    }
}
