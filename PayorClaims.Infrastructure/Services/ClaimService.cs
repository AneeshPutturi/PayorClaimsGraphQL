using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using PayorClaims.Application.Abstractions;
using PayorClaims.Application.Dtos.Claims;
using PayorClaims.Domain.Entities;
using PayorClaims.Infrastructure.Options;
using PayorClaims.Infrastructure.Persistence;

namespace PayorClaims.Infrastructure.Services;

public class ClaimService : IClaimService
{
    private readonly ClaimsDbContext _db;
    private readonly LocalStorageOptions _storage;

    public ClaimService(ClaimsDbContext db, Microsoft.Extensions.Options.IOptions<LocalStorageOptions> storage)
    {
        _db = db;
        _storage = storage.Value;
    }

    public async Task<(Claim Claim, bool AlreadyExisted)> SubmitClaimAsync(ClaimSubmissionInputDto input, string actorType, Guid? actorId, CancellationToken ct = default)
    {
        // 1) Idempotency
        var existing = await _db.Claims.AsNoTracking()
            .FirstOrDefaultAsync(c => c.IdempotencyKey == input.IdempotencyKey, ct);
        if (existing != null)
            return (existing, true);

        // 2) Coverage validation
        var coverage = await _db.Coverages.AsNoTracking()
            .Where(c => c.MemberId == input.MemberId && c.CoverageStatus == "Active"
                && c.StartDate <= input.ServiceTo
                && (c.EndDate == null || c.EndDate >= input.ServiceFrom))
            .OrderByDescending(c => c.StartDate)
            .FirstOrDefaultAsync(ct);
        if (coverage == null)
            throw new FluentValidation.ValidationException(new[] { new FluentValidation.ValidationFailure("Coverage", "No active coverage for service dates") });

        // 3) Reference validation: CPT and ICD10
        var serviceFrom = input.ServiceFrom;
        var cptCodes = input.Lines.Select(l => l.CptCode).Distinct().ToList();
        var cptExists = await _db.CptCodes.AsNoTracking()
            .Where(c => cptCodes.Contains(c.CptCodeId) && c.IsActive
                && c.EffectiveFrom <= serviceFrom
                && (c.EffectiveTo == null || c.EffectiveTo >= serviceFrom))
            .Select(c => c.CptCodeId)
            .ToListAsync(ct);
        var missingCpt = cptCodes.Except(cptExists).ToList();
        if (missingCpt.Count > 0)
            throw new FluentValidation.ValidationException(new[] { new FluentValidation.ValidationFailure("CptCode", $"CPT codes not found or not effective: {string.Join(", ", missingCpt)}") });

        var allDiagnoses = (input.Diagnoses ?? new List<ClaimDiagnosisInputDto>())
            .Concat(input.Lines.SelectMany(l => l.Diagnoses ?? new List<ClaimDiagnosisInputDto>()))
            .Where(d => d.CodeSystem == "ICD10")
            .Select(d => (d.CodeSystem, d.Code))
            .Distinct()
            .ToList();
        foreach (var (codeSystem, code) in allDiagnoses)
        {
            var exists = await _db.DiagnosisCodes.AsNoTracking()
                .AnyAsync(d => d.CodeSystem == codeSystem && d.Code == code && d.IsActive
                    && d.EffectiveFrom <= serviceFrom
                    && (d.EffectiveTo == null || d.EffectiveTo >= serviceFrom), ct);
            if (!exists)
                throw new FluentValidation.ValidationException(new[] { new FluentValidation.ValidationFailure("Diagnosis", $"Diagnosis code {codeSystem}:{code} not found or not effective.") });
        }

        // 4) Fingerprint
        var totalBilled = input.Lines.Sum(l => l.BilledAmount);
        var linePart = string.Join("|", input.Lines
            .OrderBy(l => l.LineNumber)
            .Select(l => $"{l.CptCode}:{l.Units}:{l.BilledAmount}"));
        var fingerprintInput = $"{input.MemberId}|{input.ProviderId}|{input.ServiceFrom:O}|{input.ServiceTo:O}|{totalBilled}|{linePart}";
        var fingerprint = ToHex(SHA256.HashData(Encoding.UTF8.GetBytes(fingerprintInput)));

        // 5) Claim number and insert
        var datePrefix = DateTime.UtcNow.ToString("yyyyMMdd");
        var suffix = Guid.NewGuid().ToString("N")[..4];
        var claimNumber = $"CLM{datePrefix}{suffix}";
        while (await _db.Claims.AnyAsync(c => c.ClaimNumber == claimNumber, ct))
        {
            suffix = Guid.NewGuid().ToString("N")[..4];
            claimNumber = $"CLM{datePrefix}{suffix}";
        }

        var claim = new Claim
        {
            ClaimNumber = claimNumber,
            MemberId = input.MemberId,
            ProviderId = input.ProviderId,
            CoverageId = coverage.Id,
            ReceivedDate = input.ReceivedDate,
            ServiceFromDate = input.ServiceFrom,
            ServiceToDate = input.ServiceTo,
            Status = "Received",
            TotalBilled = totalBilled,
            TotalAllowed = 0,
            TotalPaid = 0,
            Currency = "USD",
            IdempotencyKey = input.IdempotencyKey,
            DuplicateFingerprint = fingerprint,
            SubmittedByActorType = actorType,
            SubmittedByActorId = actorId
        };
        _db.Claims.Add(claim);
        await _db.SaveChangesAsync(ct);

        foreach (var lineDto in input.Lines)
        {
            var line = new ClaimLine
            {
                ClaimId = claim.Id,
                LineNumber = lineDto.LineNumber,
                CptCode = lineDto.CptCode,
                Units = lineDto.Units,
                BilledAmount = lineDto.BilledAmount,
                AllowedAmount = 0,
                PaidAmount = 0,
                LineStatus = "Pending"
            };
            _db.ClaimLines.Add(line);
        }

        foreach (var d in input.Diagnoses ?? new List<ClaimDiagnosisInputDto>())
        {
            _db.ClaimDiagnoses.Add(new ClaimDiagnosis
            {
                ClaimId = claim.Id,
                CodeSystem = d.CodeSystem,
                Code = d.Code,
                IsPrimary = d.IsPrimary,
                LineNumber = d.LineNumber
            });
        }

        foreach (var lineDto in input.Lines)
        {
            foreach (var d in lineDto.Diagnoses ?? new List<ClaimDiagnosisInputDto>())
            {
                _db.ClaimDiagnoses.Add(new ClaimDiagnosis
                {
                    ClaimId = claim.Id,
                    CodeSystem = d.CodeSystem,
                    Code = d.Code,
                    IsPrimary = d.IsPrimary,
                    LineNumber = lineDto.LineNumber
                });
            }
        }

        await _db.SaveChangesAsync(ct);
        return (claim, false);
    }

    public async Task<Claim> AdjudicateClaimAsync(Guid claimId, byte[] rowVersion, List<AdjudicateLineDto> lineDecisions, CancellationToken ct = default)
    {
        var claim = await _db.Claims.Include(c => c.Lines).Include(c => c.Coverage).FirstOrDefaultAsync(c => c.Id == claimId, ct)
            ?? throw new InvalidOperationException("Claim not found");
        _db.Entry(claim).Property(x => x.RowVersion).OriginalValue = rowVersion;

        foreach (var dec in lineDecisions)
        {
            var line = claim.Lines.FirstOrDefault(l => l.LineNumber == dec.LineNumber);
            if (line == null) continue;
            line.LineStatus = dec.Status;
            line.DenialReasonCode = dec.DenialReasonCode;
            line.AllowedAmount = dec.AllowedAmount;
            line.PaidAmount = dec.PaidAmount;
        }

        claim.TotalAllowed = claim.Lines.Sum(l => l.AllowedAmount);
        claim.TotalPaid = claim.Lines.Sum(l => l.PaidAmount);
        claim.Status = claim.Lines.All(l => l.LineStatus is "Approved" or "Denied")
            ? "Adjudicated"
            : "InReview";

        // Accumulators
        var planId = claim.Coverage?.PlanId ?? claim.CoverageId.HasValue
            ? (await _db.Coverages.AsNoTracking().Where(c => c.Id == claim.CoverageId).Select(c => c.PlanId).FirstOrDefaultAsync(ct))
            : (Guid?)null;
        if (planId.HasValue)
        {
            var year = claim.ServiceFromDate.Year;
            var acc = await _db.Accumulators.FirstOrDefaultAsync(a =>
                a.MemberId == claim.MemberId && a.PlanId == planId.Value && a.Year == year && a.Network == "InNetwork", ct);
            if (acc == null)
            {
                acc = new Accumulator { MemberId = claim.MemberId, PlanId = planId.Value, Year = year, Network = "InNetwork", DeductibleMet = 0, MoopMet = 0 };
                _db.Accumulators.Add(acc);
                await _db.SaveChangesAsync(ct);
            }
            acc.DeductibleMet += Math.Min(claim.TotalAllowed, 500); // placeholder rule
            acc.MoopMet += claim.TotalPaid;
        }

        await _db.SaveChangesAsync(ct);

        // Payment idempotently
        var payKey = "PAY-" + claim.ClaimNumber;
        var hasPayment = await _db.Payments.AnyAsync(p => p.ClaimId == claimId && p.IdempotencyKey == payKey, ct);
        if (!hasPayment)
        {
            _db.Payments.Add(new Payment
            {
                ClaimId = claim.Id,
                PaymentDate = DateTime.UtcNow,
                Amount = claim.TotalPaid,
                Method = "EFT",
                IdempotencyKey = payKey
            });
            await _db.SaveChangesAsync(ct);
        }

        // EOB + stub file
        var hasEob = await _db.Eobs.AnyAsync(e => e.ClaimId == claimId, ct);
        if (!hasEob)
        {
            var eobNumber = "EOB-" + claim.ClaimNumber;
            var localDir = Path.Combine(_storage.LocalPath, "eobs");
            Directory.CreateDirectory(localDir);
            var stubPath = Path.Combine(localDir, $"{eobNumber}.txt");
            var stubContent = $"Claim {claim.ClaimNumber} | Member {claim.MemberId} | TotalPaid {claim.TotalPaid} | Generated {DateTime.UtcNow:O}";
            await File.WriteAllTextAsync(stubPath, stubContent, ct);
            var storageKey = stubPath;
            var sha256 = ToHex(SHA256.HashData(Encoding.UTF8.GetBytes(stubContent)));

            _db.Eobs.Add(new Eob
            {
                ClaimId = claim.Id,
                EobNumber = eobNumber,
                GeneratedAt = DateTime.UtcNow,
                DocumentStorageKey = storageKey,
                DocumentSha256 = sha256,
                DeliveryMethod = "File",
                DeliveryStatus = "Pending"
            });
            await _db.SaveChangesAsync(ct);
        }

        return claim;
    }

    public async Task<ClaimAppeal> SubmitAppealAsync(Guid claimId, int level, string reason, CancellationToken ct = default)
    {
        var claim = await _db.Claims.FindAsync(new object[] { claimId }, ct) ?? throw new InvalidOperationException("Claim not found");
        var appeal = new ClaimAppeal
        {
            ClaimId = claimId,
            AppealLevel = level,
            Reason = reason,
            Status = "Submitted",
            SubmittedAt = DateTime.UtcNow
        };
        _db.ClaimAppeals.Add(appeal);
        await _db.SaveChangesAsync(ct);
        return appeal;
    }

    public async Task<ClaimAppeal> DecideAppealAsync(Guid appealId, string status, CancellationToken ct = default)
    {
        var appeal = await _db.ClaimAppeals.FindAsync(new object[] { appealId }, ct) ?? throw new InvalidOperationException("Appeal not found");
        appeal.Status = status;
        appeal.DecisionAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return appeal;
    }

    private static string ToHex(byte[] bytes)
    {
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
