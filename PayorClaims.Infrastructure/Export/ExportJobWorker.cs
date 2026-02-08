using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PayorClaims.Domain.Entities;
using PayorClaims.Infrastructure.Options;
using PayorClaims.Infrastructure.Persistence;

namespace PayorClaims.Infrastructure.Export;

public class ExportJobWorker : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(10);
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExportJobWorker> _logger;

    public ExportJobWorker(IServiceScopeFactory scopeFactory, ILogger<ExportJobWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOneJobAsync(stoppingToken);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Export job cycle failed");
            }
            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task ProcessOneJobAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ClaimsDbContext>();
        var storage = scope.ServiceProvider.GetRequiredService<IOptions<LocalStorageOptions>>().Value;
        var job = await db.ExportJobs.FirstOrDefaultAsync(j => j.Status == "Queued", ct);
        if (job == null) return;

        job.Status = "Running";
        await db.SaveChangesAsync(ct);

        try
        {
            var exportDir = Path.Combine(storage.LocalPath, "exports");
            Directory.CreateDirectory(exportDir);
            var filePath = Path.Combine(exportDir, $"{job.Id}.json");

            var claims = await db.Claims
                .AsNoTracking()
                .Where(c => c.MemberId == job.MemberId)
                .Include(c => c.Lines)
                .Include(c => c.Diagnoses)
                .OrderBy(c => c.CreatedAt)
                .Select(c => new
                {
                    c.Id,
                    c.ClaimNumber,
                    c.MemberId,
                    c.Status,
                    c.TotalBilled,
                    c.TotalAllowed,
                    c.TotalPaid,
                    c.ServiceFromDate,
                    c.ServiceToDate,
                    c.ReceivedDate,
                    Lines = c.Lines.Select(l => new { l.LineNumber, l.CptCode, l.BilledAmount, l.AllowedAmount, l.PaidAmount, l.LineStatus }),
                    Diagnoses = c.Diagnoses.Select(d => new { d.CodeSystem, d.Code, d.IsPrimary })
                })
                .ToListAsync(ct);

            var payload = new { memberId = job.MemberId, exportedAt = DateTime.UtcNow, claims };
            await using var fs = File.Create(filePath);
            await JsonSerializer.SerializeAsync(fs, payload, new JsonSerializerOptions { WriteIndented = true }, ct);

            job.Status = "Ready";
            job.CompletedAt = DateTime.UtcNow;
            job.FilePath = filePath;
            job.ExpiresAt = DateTime.UtcNow.AddHours(24);
            await db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Export job {JobId} failed", job.Id);
            job.Status = "Failed";
            job.CompletedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
    }
}
