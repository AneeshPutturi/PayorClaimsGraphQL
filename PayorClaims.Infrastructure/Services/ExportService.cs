using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using PayorClaims.Application.Abstractions;
using PayorClaims.Application.Security;
using PayorClaims.Domain.Entities;
using PayorClaims.Infrastructure.Persistence;
using PayorClaims.Infrastructure.Options;

namespace PayorClaims.Infrastructure.Services;

public class ExportService : IExportService
{
    private readonly ClaimsDbContext _db;
    private readonly LocalStorageOptions _storage;

    public ExportService(ClaimsDbContext db, Microsoft.Extensions.Options.IOptions<LocalStorageOptions> storage)
    {
        _db = db;
        _storage = storage.Value;
    }

    public async Task<ExportRequestResult> RequestMemberClaimsExportAsync(Guid memberId, ActorContext actor, CancellationToken ct = default)
    {
        var job = new ExportJob
        {
            Id = Guid.NewGuid(),
            RequestedByActorType = actor.ActorType ?? "System",
            RequestedByActorId = actor.ActorId,
            MemberId = memberId,
            Status = "Queued",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };
        _db.ExportJobs.Add(job);
        await _db.SaveChangesAsync(ct);
        return new ExportRequestResult(job.Id, job.Status, null, job.ExpiresAt);
    }

    public async Task<ExportJobStatusResult?> GetExportJobStatusAsync(Guid jobId, ActorContext? actor, CancellationToken ct = default)
    {
        var job = await _db.ExportJobs.FirstOrDefaultAsync(j => j.Id == jobId, ct);
        if (job == null) return null;
        string? tokenOnce = null;
        if (job.Status == "Ready" && job.DownloadTokenHash == null && job.ExpiresAt > DateTime.UtcNow)
        {
            var token = $"{job.Id}:{Guid.NewGuid():N}";
            job.DownloadTokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();
            job.ExpiresAt = DateTime.UtcNow.AddHours(24);
            await _db.SaveChangesAsync(ct);
            tokenOnce = token;
        }
        return new ExportJobStatusResult(job.Status, job.CompletedAt, tokenOnce, job.ExpiresAt);
    }
}
