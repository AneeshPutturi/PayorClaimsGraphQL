using PayorClaims.Domain.Entities;

namespace PayorClaims.Application.Abstractions;

public interface IClaimAttachmentService
{
    Task<ClaimAttachment> UploadAsync(Guid claimId, string fileName, string contentType, byte[] data, string actorType, Guid? actorId, CancellationToken ct = default);
}
