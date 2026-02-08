using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PayorClaims.Application.Abstractions;
using PayorClaims.Domain.Entities;
using PayorClaims.Infrastructure.Persistence;

namespace PayorClaims.Infrastructure.Seed;

public class SeedRunner : ISeedRunner
{
    private readonly ClaimsDbContext _db;
    private readonly ILogger<SeedRunner> _logger;
    private readonly IConfiguration _config;
    private readonly IHostEnvironment _env;

    private const string Placeholder64Hex = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    private static readonly DateOnly SeedEffective = new(2026, 1, 1);

    public SeedRunner(
        ClaimsDbContext db,
        ILogger<SeedRunner> logger,
        IConfiguration config,
        IHostEnvironment env)
    {
        _db = db;
        _logger = logger;
        _config = config;
        _env = env;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        var conn = _db.Database.GetDbConnection();
        _logger.LogInformation("SEED START");
        _logger.LogInformation("Database name: {Database}, Server: {DataSource}", conn.Database, conn.DataSource);

        await _db.Database.MigrateAsync(ct);
        _logger.LogInformation("MIGRATIONS DONE");

        if (string.Equals(_config["Seed:Reset"], "true", StringComparison.OrdinalIgnoreCase) && _env.IsDevelopment())
        {
            _logger.LogWarning("Seed:Reset=true in Development — deleting data in reverse dependency order.");
            await ResetDataAsync(ct);
        }

        await SeedCoreAsync(ct);
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("SEED DONE");

        var plansCount = await _db.Plans.IgnoreQueryFilters().CountAsync(ct);
        var membersCount = await _db.Members.IgnoreQueryFilters().CountAsync(ct);
        var providersCount = await _db.Providers.IgnoreQueryFilters().CountAsync(ct);
        var claimsCount = await _db.Claims.IgnoreQueryFilters().CountAsync(ct);
        _logger.LogInformation("Counts: Plans={Plans} Members={Members} Providers={Providers} Claims={Claims}", plansCount, membersCount, providersCount, claimsCount);

        if (plansCount == 0 || membersCount == 0 || providersCount == 0)
            throw new InvalidOperationException($"Seed did not insert required data. Plans={plansCount} Members={membersCount} Providers={providersCount}. Check connection and seed logic.");
    }

    private async Task ResetDataAsync(CancellationToken ct)
    {
        await _db.HipaaAccessLogs.ExecuteDeleteAsync(ct);
        await _db.AuditEvents.IgnoreQueryFilters().ExecuteDeleteAsync(ct);
        await _db.MemberConsents.IgnoreQueryFilters().ExecuteDeleteAsync(ct);
        await _db.Eobs.IgnoreQueryFilters().ExecuteDeleteAsync(ct);
        await _db.Payments.IgnoreQueryFilters().ExecuteDeleteAsync(ct);
        await _db.ClaimDiagnoses.IgnoreQueryFilters().ExecuteDeleteAsync(ct);
        await _db.ClaimLines.IgnoreQueryFilters().ExecuteDeleteAsync(ct);
        await _db.ClaimAttachments.IgnoreQueryFilters().ExecuteDeleteAsync(ct);
        await _db.ClaimAppeals.IgnoreQueryFilters().ExecuteDeleteAsync(ct);
        await _db.Claims.IgnoreQueryFilters().ExecuteDeleteAsync(ct);
        await _db.PriorAuths.IgnoreQueryFilters().ExecuteDeleteAsync(ct);
        await _db.Accumulators.IgnoreQueryFilters().ExecuteDeleteAsync(ct);
        await _db.MemberInsurancePolicies.IgnoreQueryFilters().ExecuteDeleteAsync(ct);
        await _db.Coverages.IgnoreQueryFilters().ExecuteDeleteAsync(ct);
        await _db.PlanBenefits.IgnoreQueryFilters().ExecuteDeleteAsync(ct);
        await _db.ProviderLocations.IgnoreQueryFilters().ExecuteDeleteAsync(ct);
        await _db.Providers.IgnoreQueryFilters().ExecuteDeleteAsync(ct);
        await _db.Plans.IgnoreQueryFilters().ExecuteDeleteAsync(ct);
        await _db.Members.IgnoreQueryFilters().ExecuteDeleteAsync(ct);
        await _db.CptCodes.ExecuteDeleteAsync(ct);
        await _db.DiagnosisCodes.ExecuteDeleteAsync(ct);
        await _db.AdjustmentReasonCodes.ExecuteDeleteAsync(ct);
    }

    private async Task SeedCoreAsync(CancellationToken ct)
    {
        // Minimum seed: ensure at least 1 plan, 1 provider, 1 member exist (fail loudly if not)
        if (!await _db.Plans.IgnoreQueryFilters().AnyAsync(ct))
            _db.Plans.Add(new Plan { PlanCode = "PPO-SILVER", Name = "PPO Silver", Year = 2026, NetworkType = "PPO", MetalTier = "Silver", IsActive = true });
        if (!await _db.Providers.IgnoreQueryFilters().AnyAsync(ct))
            _db.Providers.Add(new Provider { Npi = "1111111111", Name = "Test Provider", ProviderType = "Individual", ProviderStatus = "Active" });
        if (!await _db.Members.IgnoreQueryFilters().AnyAsync(ct))
            _db.Members.Add(new Member { ExternalMemberNumber = "MEM0001", FirstName = "Test", LastName = "Member", Dob = new DateOnly(1990, 1, 1), Status = "Active" });
        await _db.SaveChangesAsync(ct);

        _logger.LogDebug("Seeding adjustment reason codes, CPT codes, diagnosis codes...");
        await SeedAdjustmentReasonCodesAsync(ct);
        await SeedCptCodesAsync(ct);
        await SeedDiagnosisCodesAsync(ct);
        _logger.LogDebug("Seeding plans and benefits...");
        await SeedPlansAndBenefitsAsync(ct);
        _logger.LogDebug("Seeding providers and locations...");
        await SeedProvidersAndLocationsAsync(ct);
        _logger.LogDebug("Seeding members and coverages...");
        await SeedMembersAndCoveragesAsync(ct);
        await SeedMemberInsurancePoliciesAsync(ct);
        await SeedAccumulatorsAsync(ct);
        await SeedPriorAuthsAsync(ct);
        _logger.LogDebug("Seeding claim, payment, EOB, consents, audit, HIPAA log...");
        await SeedClaimWithLinesAndDiagnosesAsync(ct);
        await SeedPaymentAsync(ct);
        await SeedEobAsync(ct);
        await SeedMemberConsentsAsync(ct);
        await SeedAuditEventAsync(ct);
        await SeedHipaaAccessLogAsync(ct);
    }

    private async Task SeedAdjustmentReasonCodesAsync(CancellationToken ct)
    {
        var codes = new[]
        {
            ("CO", "CARC", "Patient responsibility (Coinsurance)", true),
            ("PR", "CARC", "Patient responsibility (Amount)", true),
            ("OA", "RARC", "Missing/incomplete diagnosis", true),
            ("MA", "RARC", "Missing/incomplete prior auth", true),
            ("SEED", "INTERNAL", "Seed denial reason", true),
        };
        foreach (var (code, codeType, desc, active) in codes)
        {
            if (await _db.AdjustmentReasonCodes.AnyAsync(x => x.Code == code, ct)) continue;
            _db.AdjustmentReasonCodes.Add(new AdjustmentReasonCode
            {
                Code = code,
                CodeType = codeType,
                Description = desc,
                IsActive = active,
            });
        }
        await _db.SaveChangesAsync(ct);
    }

    private async Task SeedCptCodesAsync(CancellationToken ct)
    {
        var items = new[]
        {
            ("99213", "Office visit, established patient, 20-29 min"),
            ("99214", "Office visit, established patient, 30-39 min"),
            ("99285", "Emergency dept visit, high severity"),
            ("99284", "Emergency dept visit, moderate severity"),
            ("80053", "Comprehensive metabolic panel"),
            ("85025", "CBC with differential"),
            ("93000", "EKG complete"),
            ("71046", "Chest X-ray single view"),
            ("J0696", "Injection ceftriaxone 250mg"),
            ("J2710", "Injection naloxone 1mg"),
        };
        foreach (var (id, desc) in items)
        {
            if (await _db.CptCodes.AnyAsync(x => x.CptCodeId == id, ct)) continue;
            _db.CptCodes.Add(new CptCode
            {
                CptCodeId = id,
                Description = desc,
                EffectiveFrom = SeedEffective,
                IsActive = true,
            });
        }
        await _db.SaveChangesAsync(ct);
    }

    private async Task SeedDiagnosisCodesAsync(CancellationToken ct)
    {
        var items = new[]
        {
            ("E11.9", "Type 2 diabetes without complications"),
            ("I10", "Essential hypertension"),
            ("J06.9", "Acute upper respiratory infection"),
            ("M54.5", "Low back pain"),
            ("R10.9", "Unspecified abdominal pain"),
            ("G89.29", "Other chronic pain"),
            ("F41.1", "Generalized anxiety disorder"),
            ("K21.9", "GERD without esophagitis"),
            ("E78.00", "Hypercholesterolemia unspecified"),
            ("J44.9", "COPD unspecified"),
        };
        foreach (var (code, desc) in items)
        {
            if (await _db.DiagnosisCodes.AnyAsync(x => x.CodeSystem == "ICD10" && x.Code == code, ct)) continue;
            _db.DiagnosisCodes.Add(new DiagnosisCode
            {
                CodeSystem = "ICD10",
                Code = code,
                Description = desc,
                EffectiveFrom = SeedEffective,
                IsActive = true,
            });
        }
        await _db.SaveChangesAsync(ct);
    }

    private async Task SeedPlansAndBenefitsAsync(CancellationToken ct)
    {
        var planSpecs = new[]
        {
            (PlanCode: "PPO-SLV-26", Name: "2026 PPO Silver", NetworkType: "PPO", MetalTier: "Silver"),
            (PlanCode: "HMO-GLD-26", Name: "2026 HMO Gold", NetworkType: "HMO", MetalTier: "Gold"),
        };
        var planIds = new List<Guid>();
        foreach (var (planCode, name, networkType, metalTier) in planSpecs)
        {
            var existing = await _db.Plans.IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.PlanCode == planCode && p.Year == 2026, ct);
            if (existing != null)
            {
                planIds.Add(existing.Id);
                continue;
            }
            var plan = new Plan
            {
                PlanCode = planCode,
                Name = name,
                Year = 2026,
                NetworkType = networkType,
                MetalTier = metalTier,
                IsActive = true,
            };
            _db.Plans.Add(plan);
            await _db.SaveChangesAsync(ct);
            planIds.Add(plan.Id);
        }

        var benefitCategories = new[]
        {
            (Category: "Deductible", Network: "InNetwork", CoverageLevel: "Individual", Period: "Annual", Copay: (decimal?)null, Coins: (decimal?)null, DeductibleApplies: true, MaxVisits: (int?)null),
            (Category: "Deductible", Network: "InNetwork", CoverageLevel: "Family", Period: "Annual", Copay: (decimal?)null, Coins: (decimal?)null, DeductibleApplies: true, MaxVisits: (int?)null),
            (Category: "MOOP", Network: "InNetwork", CoverageLevel: "Individual", Period: "Annual", Copay: (decimal?)null, Coins: (decimal?)null, DeductibleApplies: false, MaxVisits: (int?)null),
            (Category: "OfficeVisit", Network: "InNetwork", CoverageLevel: "Individual", Period: "PerVisit", Copay: 25m, Coins: (decimal?)null, DeductibleApplies: true, MaxVisits: (int?)null),
            (Category: "ER", Network: "InNetwork", CoverageLevel: "Individual", Period: "PerVisit", Copay: 250m, Coins: (decimal?)null, DeductibleApplies: true, MaxVisits: (int?)null),
            (Category: "RxGeneric", Network: "InNetwork", CoverageLevel: "Individual", Period: "PerFill", Copay: 10m, Coins: (decimal?)null, DeductibleApplies: false, MaxVisits: (int?)null),
        };

        foreach (var planId in planIds)
        {
            var plan = await _db.Plans.IgnoreQueryFilters().FirstAsync(p => p.Id == planId, ct);
            foreach (var b in benefitCategories)
            {
                var exists = await _db.PlanBenefits.IgnoreQueryFilters()
                    .AnyAsync(x => x.PlanId == planId && x.Category == b.Category && x.EffectiveFrom == SeedEffective, ct);
                if (exists) continue;
                _db.PlanBenefits.Add(new PlanBenefit
                {
                    PlanId = planId,
                    BenefitVersion = 1,
                    EffectiveFrom = SeedEffective,
                    Category = b.Category,
                    Network = b.Network,
                    CoverageLevel = b.CoverageLevel,
                    Period = b.Period,
                    CopayAmount = b.Copay,
                    CoinsurancePercent = b.Coins,
                    DeductibleApplies = b.DeductibleApplies,
                    MaxVisits = b.MaxVisits,
                });
            }
        }
        await _db.SaveChangesAsync(ct);
    }

    private async Task SeedProvidersAndLocationsAsync(CancellationToken ct)
    {
        var specs = new[]
        {
            (Npi: "1234567890", Name: "Jane Smith MD", ProviderType: "Individual", Specialty: "Internal Medicine"),
            (Npi: "2345678901", Name: "John Doe DO", ProviderType: "Individual", Specialty: "Family Medicine"),
            (Npi: "3456789012", Name: "Metro General Hospital", ProviderType: "Facility", Specialty: (string?)null),
        };
        var providerIds = new List<Guid>();
        foreach (var (npi, name, providerType, specialty) in specs)
        {
            var existing = await _db.Providers.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Npi == npi, ct);
            if (existing != null)
            {
                providerIds.Add(existing.Id);
                continue;
            }
            var provider = new Provider
            {
                Npi = npi,
                Name = name,
                ProviderType = providerType,
                Specialty = specialty,
                ProviderStatus = "Active",
                CredentialedFrom = SeedEffective,
            };
            _db.Providers.Add(provider);
            await _db.SaveChangesAsync(ct);
            providerIds.Add(provider.Id);
        }

        var cities = new[] { "Seattle", "Portland", "Seattle" };
        for (var i = 0; i < providerIds.Count; i++)
        {
            var providerId = providerIds[i];
            var hasLocation = await _db.ProviderLocations.IgnoreQueryFilters().AnyAsync(x => x.ProviderId == providerId, ct);
            if (hasLocation) continue;
            _db.ProviderLocations.Add(new ProviderLocation
            {
                ProviderId = providerId,
                AddressLine1 = $"{100 + i} Medical Center Dr",
                City = cities[i],
                State = "WA",
                Zip = "98101",
                IsPrimary = true,
            });
        }
        await _db.SaveChangesAsync(ct);
    }

    private async Task SeedMembersAndCoveragesAsync(CancellationToken ct)
    {
        var planIds = await _db.Plans.IgnoreQueryFilters().Where(p => p.Year == 2026).Select(p => p.Id).ToListAsync(ct);
        if (planIds.Count < 2) return;

        for (var i = 1; i <= 5; i++)
        {
            var ext = $"MEM{i:D4}";
            var existing = await _db.Members.IgnoreQueryFilters().FirstOrDefaultAsync(m => m.ExternalMemberNumber == ext, ct);
            Member member;
            if (existing != null)
            {
                member = existing;
            }
            else
            {
                member = new Member
                {
                    ExternalMemberNumber = ext,
                    FirstName = $"First{i}",
                    LastName = $"Last{i}",
                    Dob = new DateOnly(1980 + i, 1, 15),
                    Gender = i % 2 == 0 ? "F" : "M",
                    Status = "Active",
                };
                _db.Members.Add(member);
                await _db.SaveChangesAsync(ct);
            }

            var planId = planIds[i % 2];
            var hasCoverage = await _db.Coverages.IgnoreQueryFilters()
                .AnyAsync(c => c.MemberId == member.Id && c.PlanId == planId && c.StartDate == SeedEffective, ct);
            if (hasCoverage) continue;
            _db.Coverages.Add(new Coverage
            {
                MemberId = member.Id,
                PlanId = planId,
                StartDate = SeedEffective,
                EndDate = null,
                CoverageStatus = "Active",
            });
        }
        await _db.SaveChangesAsync(ct);
    }

    private async Task SeedMemberInsurancePoliciesAsync(CancellationToken ct)
    {
        var mem = await _db.Members.IgnoreQueryFilters().FirstOrDefaultAsync(m => m.ExternalMemberNumber == "MEM0002", ct);
        if (mem == null) return;
        var exists = await _db.MemberInsurancePolicies.IgnoreQueryFilters()
            .AnyAsync(p => p.MemberId == mem.Id && p.Priority == 2, ct);
        if (exists) return;
        _db.MemberInsurancePolicies.Add(new MemberInsurancePolicy
        {
            MemberId = mem.Id,
            PayerName = "Other Payer",
            PolicyNumber = "SEC-2026-001",
            Priority = 2,
            EffectiveFrom = SeedEffective,
        });
        await _db.SaveChangesAsync(ct);
    }

    private async Task SeedAccumulatorsAsync(CancellationToken ct)
    {
        var mem = await _db.Members.IgnoreQueryFilters().FirstOrDefaultAsync(m => m.ExternalMemberNumber == "MEM0001", ct);
        if (mem == null) return;
        var plan = await _db.Plans.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Year == 2026, ct);
        if (plan == null) return;
        var exists = await _db.Accumulators.IgnoreQueryFilters()
            .AnyAsync(a => a.MemberId == mem.Id && a.PlanId == plan.Id && a.Year == 2026 && a.Network == "InNetwork", ct);
        if (exists) return;
        _db.Accumulators.Add(new Accumulator
        {
            MemberId = mem.Id,
            PlanId = plan.Id,
            Year = 2026,
            Network = "InNetwork",
            DeductibleMet = 150m,
            MoopMet = 50m,
        });
        await _db.SaveChangesAsync(ct);
    }

    private async Task SeedPriorAuthsAsync(CancellationToken ct)
    {
        var mem = await _db.Members.IgnoreQueryFilters().FirstOrDefaultAsync(m => m.ExternalMemberNumber == "MEM0001", ct);
        var provider = await _db.Providers.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Npi == "1234567890", ct);
        if (mem == null || provider == null) return;

        var requestedAt = new DateTime(2026, 1, 10, 9, 0, 0, DateTimeKind.Utc);
        var requestedExists = await _db.PriorAuths.IgnoreQueryFilters()
            .AnyAsync(pa => pa.MemberId == mem.Id && pa.ServiceType == "Surgery" && pa.RequestedDate == requestedAt, ct);
        if (!requestedExists)
        {
            _db.PriorAuths.Add(new PriorAuth
            {
                MemberId = mem.Id,
                ProviderId = provider.Id,
                ServiceType = "Surgery",
                RequestedDate = requestedAt,
                Status = "Requested",
            });
        }

        var approvedAt = new DateTime(2026, 1, 5, 14, 0, 0, DateTimeKind.Utc);
        var approvedExists = await _db.PriorAuths.IgnoreQueryFilters()
            .AnyAsync(pa => pa.MemberId == mem.Id && pa.ServiceType == "MRI" && pa.Status == "Approved", ct);
        if (!approvedExists)
        {
            _db.PriorAuths.Add(new PriorAuth
            {
                MemberId = mem.Id,
                ProviderId = provider.Id,
                ServiceType = "MRI",
                RequestedDate = approvedAt.AddDays(-2),
                DecisionDate = approvedAt,
                Status = "Approved",
            });
        }
        await _db.SaveChangesAsync(ct);
    }

    private async Task SeedClaimWithLinesAndDiagnosesAsync(CancellationToken ct)
    {
        if (await _db.Claims.IgnoreQueryFilters().AnyAsync(c => c.ClaimNumber == "CLM0001", ct)) return;

        var member = await _db.Members.IgnoreQueryFilters().FirstAsync(m => m.ExternalMemberNumber == "MEM0001", ct);
        var provider = await _db.Providers.IgnoreQueryFilters().FirstAsync(p => p.Npi == "1234567890", ct);
        var coverage = await _db.Coverages.IgnoreQueryFilters()
            .FirstAsync(c => c.MemberId == member.Id && c.StartDate == SeedEffective, ct);
        var diagCodes = await _db.DiagnosisCodes.Where(d => d.CodeSystem == "ICD10").OrderBy(d => d.Code).Take(3).Select(d => d.Code).ToListAsync(ct);
        var denialCode = await _db.AdjustmentReasonCodes.FirstAsync(x => x.Code == "SEED", ct);

        var claim = new Claim
        {
            ClaimNumber = "CLM0001",
            MemberId = member.Id,
            ProviderId = provider.Id,
            CoverageId = coverage.Id,
            ReceivedDate = new DateOnly(2026, 2, 1),
            ServiceFromDate = new DateOnly(2026, 1, 28),
            ServiceToDate = new DateOnly(2026, 1, 28),
            Status = "Received",
            TotalBilled = 0,
            TotalAllowed = 0,
            TotalPaid = 0,
            Currency = "USD",
            DuplicateFingerprint = Placeholder64Hex,
            SubmittedByActorType = "Provider",
            SubmittedByActorId = provider.Id,
        };

        var line1 = new ClaimLine
        {
            Claim = claim,
            LineNumber = 1,
            CptCode = "99213",
            Units = 1,
            BilledAmount = 150m,
            AllowedAmount = 120m,
            PaidAmount = 120m,
            LineStatus = "Approved",
        };
        var line2 = new ClaimLine
        {
            Claim = claim,
            LineNumber = 2,
            CptCode = "80053",
            Units = 1,
            BilledAmount = 80m,
            AllowedAmount = 0m,
            PaidAmount = 0m,
            LineStatus = "Denied",
            DenialReasonCode = denialCode.Code,
            DenialReasonText = "Seed denial",
        };
        claim.TotalBilled = line1.BilledAmount + line2.BilledAmount;
        claim.TotalAllowed = line1.AllowedAmount + line2.AllowedAmount;
        claim.TotalPaid = line1.PaidAmount + line2.PaidAmount;

        claim.Lines.Add(line1);
        claim.Lines.Add(line2);

        claim.Diagnoses.Add(new ClaimDiagnosis { Claim = claim, LineNumber = null, CodeSystem = "ICD10", Code = diagCodes[0], IsPrimary = true });
        claim.Diagnoses.Add(new ClaimDiagnosis { Claim = claim, LineNumber = 1, CodeSystem = "ICD10", Code = diagCodes[0], IsPrimary = true });
        claim.Diagnoses.Add(new ClaimDiagnosis { Claim = claim, LineNumber = 2, CodeSystem = "ICD10", Code = diagCodes[1], IsPrimary = true });

        _db.Claims.Add(claim);
        await _db.SaveChangesAsync(ct);
    }

    private async Task SeedPaymentAsync(CancellationToken ct)
    {
        if (await _db.Payments.IgnoreQueryFilters().AnyAsync(p => p.IdempotencyKey == "PAY-CLM0001-1", ct)) return;
        var claim = await _db.Claims.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.ClaimNumber == "CLM0001", ct);
        if (claim == null) return;

        _db.Payments.Add(new Payment
        {
            ClaimId = claim.Id,
            PaymentDate = new DateTime(2026, 2, 5, 0, 0, 0, DateTimeKind.Utc),
            Amount = claim.TotalPaid,
            Method = "EFT",
            IdempotencyKey = "PAY-CLM0001-1",
        });
        await _db.SaveChangesAsync(ct);
    }

    private async Task SeedEobAsync(CancellationToken ct)
    {
        if (await _db.Eobs.IgnoreQueryFilters().AnyAsync(e => e.EobNumber == "EOB0001", ct)) return;
        var claim = await _db.Claims.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.ClaimNumber == "CLM0001", ct);
        if (claim == null) return;

        _db.Eobs.Add(new Eob
        {
            ClaimId = claim.Id,
            EobNumber = "EOB0001",
            GeneratedAt = new DateTime(2026, 2, 5, 0, 0, 0, DateTimeKind.Utc),
            DocumentStorageKey = "local/eobs/EOB0001.pdf",
            DocumentSha256 = Placeholder64Hex,
            DeliveryMethod = "Portal",
            DeliveryStatus = "Pending",
        });
        await _db.SaveChangesAsync(ct);
    }

    private async Task SeedMemberConsentsAsync(CancellationToken ct)
    {
        var mem = await _db.Members.IgnoreQueryFilters().FirstOrDefaultAsync(m => m.ExternalMemberNumber == "MEM0001", ct);
        if (mem == null) return;

        if (!await _db.MemberConsents.IgnoreQueryFilters().AnyAsync(c => c.MemberId == mem.Id && c.ConsentType == "DataShare", ct))
        {
            _db.MemberConsents.Add(new MemberConsent
            {
                MemberId = mem.Id,
                ConsentType = "DataShare",
                Granted = true,
                GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Source = "Portal",
            });
        }
        if (!await _db.MemberConsents.IgnoreQueryFilters().AnyAsync(c => c.MemberId == mem.Id && c.ConsentType == "Email", ct))
        {
            _db.MemberConsents.Add(new MemberConsent
            {
                MemberId = mem.Id,
                ConsentType = "Email",
                Granted = false,
                GrantedAt = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc),
                RevokedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Source = "Portal",
            });
        }
        await _db.SaveChangesAsync(ct);
    }

    private async Task SeedAuditEventAsync(CancellationToken ct)
    {
        var claim = await _db.Claims.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.ClaimNumber == "CLM0001", ct);
        if (claim == null) return;
        var exists = await _db.AuditEvents.IgnoreQueryFilters()
            .AnyAsync(e => e.EntityType == "Claim" && e.EntityId == claim.Id && e.Action == "Created", ct);
        if (exists) return;

        _db.AuditEvents.Add(new AuditEvent
        {
            ActorUserId = "seed-system",
            Action = "Created",
            EntityType = "Claim",
            EntityId = claim.Id,
            OccurredAt = new DateTime(2026, 2, 1, 12, 0, 0, DateTimeKind.Utc),
            Notes = "Claim submitted",
        });
        await _db.SaveChangesAsync(ct);
    }

    private async Task SeedHipaaAccessLogAsync(CancellationToken ct)
    {
        var count = await _db.HipaaAccessLogs.CountAsync(ct);
        if (count >= 2) return;

        var prevHash = count == 0 ? new string('0', 64) : (await _db.HipaaAccessLogs.OrderBy(h => h.OccurredAt).LastAsync(ct)).Hash;
        var occurred1 = new DateTime(2026, 2, 1, 12, 0, 0, DateTimeKind.Utc);
        var payload1 = $"{prevHash}|System|{Guid.Empty}|View|Claim|{Guid.Empty}|{occurred1:o}|Treatment";
        var hash1 = ComputeSha256Hex(payload1);

        if (count == 0)
        {
            _db.HipaaAccessLogs.Add(new HipaaAccessLog
            {
                AccessLogId = Guid.NewGuid(),
                ActorType = "System",
                ActorId = Guid.Empty,
                Action = "View",
                SubjectType = "Claim",
                SubjectId = Guid.Empty,
                OccurredAt = occurred1,
                PurposeOfUse = "Treatment",
                PrevHash = prevHash,
                Hash = hash1,
            });
            await _db.SaveChangesAsync(ct);
        }

        var last = await _db.HipaaAccessLogs.OrderBy(h => h.OccurredAt).LastAsync(ct);
        prevHash = last.Hash;
        var occurred2 = last.OccurredAt.AddMinutes(1);
        var payload2 = $"{prevHash}|User|{Guid.Empty}|View|Member|{Guid.Empty}|{occurred2:o}|Treatment";
        var hash2 = ComputeSha256Hex(payload2);

        _db.HipaaAccessLogs.Add(new HipaaAccessLog
        {
            AccessLogId = Guid.NewGuid(),
            ActorType = "User",
            ActorId = Guid.Empty,
            Action = "View",
            SubjectType = "Member",
            SubjectId = Guid.Empty,
            OccurredAt = occurred2,
            PurposeOfUse = "Treatment",
            PrevHash = prevHash,
            Hash = hash2,
        });
        await _db.SaveChangesAsync(ct);
    }

    private static string ComputeSha256Hex(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
