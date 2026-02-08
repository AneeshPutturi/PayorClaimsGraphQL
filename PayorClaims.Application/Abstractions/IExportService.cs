using PayorClaims.Application.Security;

namespace PayorClaims.Application.Abstractions;

public record ExportRequestResult(Guid JobId, string Status, string? DownloadTokenOnce, DateTime? ExpiresAt);

public record ExportJobStatusResult(string Status, DateTime? ReadyAt, string? DownloadTokenOnce, DateTime? ExpiresAt);

public interface IExportService
{
    Task<ExportRequestResult> RequestMemberClaimsExportAsync(Guid memberId, ActorContext actor, CancellationToken ct = default);
    Task<ExportJobStatusResult?> GetExportJobStatusAsync(Guid jobId, ActorContext? actor, CancellationToken ct = default);
}
