namespace PayorClaims.Application.Security;

public interface IConsentService
{
    Task<bool> HasConsentAsync(Guid memberId, string consentType, CancellationToken ct = default);
}
