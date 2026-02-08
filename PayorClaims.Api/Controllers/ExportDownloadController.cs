using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayorClaims.Api.Middleware;
using PayorClaims.Application.Security;
using PayorClaims.Infrastructure.Options;
using PayorClaims.Infrastructure.Persistence;

namespace PayorClaims.Api.Controllers;

[ApiController]
[Route("exports")]
public class ExportDownloadController : ControllerBase
{
    [HttpGet("{jobId:guid}/download")]
    [Authorize]
    public async Task<IActionResult> Download(
        [FromRoute] Guid jobId,
        [FromQuery] string? token,
        [FromServices] ClaimsDbContext db,
        [FromServices] Microsoft.Extensions.Options.IOptions<LocalStorageOptions> storage,
        CancellationToken ct)
    {
        if (string.IsNullOrEmpty(token))
            return BadRequest("Missing token");
        var actor = HttpContext.Items[ActorContext.HttpContextItemKey] as ActorContext;
        var job = await db.ExportJobs.AsNoTracking().FirstOrDefaultAsync(j => j.Id == jobId, ct);
        if (job == null)
            return NotFound();
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();
        if (job.DownloadTokenHash != hash)
            return Unauthorized("Invalid token");
        if (job.ExpiresAt == null || job.ExpiresAt < DateTime.UtcNow)
            return Unauthorized("Export link expired");
        if (job.Status != "Ready" || string.IsNullOrEmpty(job.FilePath))
            return NotFound("Export not ready");
        if (actor != null && !actor.IsAdmin && (actor.ActorId != job.RequestedByActorId || actor.ActorType != job.RequestedByActorType))
            return Forbid();
        if (!System.IO.File.Exists(job.FilePath))
            return NotFound("File no longer available");
        var fileName = $"claims-export-{job.MemberId}.json";
        return PhysicalFile(job.FilePath, "application/json", fileName, enableRangeProcessing: false);
    }
}
