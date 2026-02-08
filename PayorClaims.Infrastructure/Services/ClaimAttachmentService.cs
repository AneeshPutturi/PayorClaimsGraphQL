using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using PayorClaims.Application.Abstractions;
using PayorClaims.Application.Exceptions;
using PayorClaims.Domain.Entities;
using PayorClaims.Infrastructure.Options;
using PayorClaims.Infrastructure.Persistence;

namespace PayorClaims.Infrastructure.Services;

public class ClaimAttachmentService : IClaimAttachmentService
{
    private const int MaxAttachmentBytes = 5 * 1024 * 1024; // 5 MB
    private readonly ClaimsDbContext _db;
    private readonly LocalStorageOptions _storage;

    public ClaimAttachmentService(ClaimsDbContext db, Microsoft.Extensions.Options.IOptions<LocalStorageOptions> storage)
    {
        _db = db;
        _storage = storage.Value;
    }

    public async Task<ClaimAttachment> UploadAsync(Guid claimId, string fileName, string contentType, byte[] data, string actorType, Guid? actorId, CancellationToken ct = default)
    {
        if (data.Length > MaxAttachmentBytes)
            throw new AppValidationException("ATTACHMENT_TOO_LARGE", $"Attachment size exceeds 5 MB (got {data.Length} bytes).");

        var claimExists = await _db.Claims.AsNoTracking().AnyAsync(c => c.Id == claimId, ct);
        if (!claimExists)
            throw new AppValidationException("VALIDATION_FAILED", "Claim not found");

        var sha256 = Convert.ToHexString(SHA256.HashData(data)).ToLowerInvariant();
        var baseDir = Path.Combine(_storage.LocalPath);
        Directory.CreateDirectory(baseDir);
        var attachmentsDir = Path.Combine(baseDir, "attachments");
        Directory.CreateDirectory(attachmentsDir);
        var eobsDir = Path.Combine(baseDir, "eobs");
        Directory.CreateDirectory(eobsDir);

        var storageKey = Path.Combine(attachmentsDir, $"{sha256}.bin");
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
