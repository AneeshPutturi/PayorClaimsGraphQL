using PayorClaims.Application.Dtos.Claims;
using PayorClaims.Domain.Entities;

namespace PayorClaims.Application.Abstractions;

public interface IClaimService
{
    Task<(Claim Claim, bool AlreadyExisted)> SubmitClaimAsync(ClaimSubmissionInputDto input, string actorType, Guid? actorId, CancellationToken ct = default);
    Task<Claim> AdjudicateClaimAsync(Guid claimId, byte[] rowVersion, List<AdjudicateLineDto> lineDecisions, CancellationToken ct = default);
    Task<ClaimAppeal> SubmitAppealAsync(Guid claimId, int level, string reason, CancellationToken ct = default);
    Task<ClaimAppeal> DecideAppealAsync(Guid appealId, string status, CancellationToken ct = default);
}
