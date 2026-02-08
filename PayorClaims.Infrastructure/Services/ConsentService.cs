using Microsoft.EntityFrameworkCore;
using PayorClaims.Application.Security;
using PayorClaims.Infrastructure.Persistence;

namespace PayorClaims.Infrastructure.Services;

public class ConsentService : IConsentService
{
    private readonly ClaimsDbContext _db;

    public ConsentService(ClaimsDbContext db) => _db = db;

    public async Task<bool> HasConsentAsync(Guid memberId, string consentType, CancellationToken ct = default)
    {
        return await _db.MemberConsents.AsNoTracking()
            .AnyAsync(c => c.MemberId == memberId && c.ConsentType == consentType && c.Granted && c.RevokedAt == null, ct);
    }
}
