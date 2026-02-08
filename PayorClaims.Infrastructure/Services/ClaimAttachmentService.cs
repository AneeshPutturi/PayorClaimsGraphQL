using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using PayorClaims.Application.Abstractions;
using PayorClaims.Domain.Entities;
using PayorClaims.Infrastructure.Options;
using PayorClaims.Infrastructure.Persistence;

namespace PayorClaims.Infrastructure.Services;

public class ClaimAttachmentService : IClaimAttachmentService
{
    private readonly ClaimsDbContext _db;
    private readonly LocalStorageOptions _storage;

    public ClaimAttachmentService(ClaimsDbContext db, Microsoft.Extensions.Options.IOptions<LocalStorageOptions> storage)
    {
        _db = db;
        _storage = storage.Value;
    }

    public async Task<ClaimAttachment> UploadAsync(Guid claimId, string fileName, string contentType, byte[] data, string actorType, Guid? actorId, CancellationToken ct = default)
    {
        var claim = await _db.Claims.AsNoTracking().AnyAsync(c => c.Id == claimId, ct);
        if (!claim)
            throw new InvalidOperationException("Claim not found");

        var sha256 = Convert.ToHexString(SHA256.HashData(data)).ToLowerInvariant();
        var localDir = Path.Combine(_storage.LocalPath, "attachments");
        Directory.CreateDirectory(localDir);
        var storageKey = Path.Combine(localDir, $"{sha256}.bin");
        await File.WriteAllBytesAsync(storageKey, data, ct);

        var attachment = new ClaimAttachment
        {
            ClaimId = claimId,
            FileName = fileName,
            ContentType = contentType,
            StorageProvider = "Local",
            StorageKey = storageKey,
            Sha256 = sha256,
            UploadedByActorType = actorType,
            UploadedByActorId = actorId,
            UploadedAt = DateTime.UtcNow
        };
        _db.ClaimAttachments.Add(attachment);
        await _db.SaveChangesAsync(ct);
        return attachment;
    }
}
