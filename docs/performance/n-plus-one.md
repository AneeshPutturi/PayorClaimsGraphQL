# N+1 and DataLoader strategy

How the schema avoids N+1 query problems using GraphQL.NET DataLoader, which loaders exist, rules for adding new ones, and how to verify that N+1 is solved.

---

## Why DataLoader exists

In GraphQL, a single query can request the same related data for many parents (e.g. plan for each of 20 coverages). Without batching, that would issue 20 separate DB queries (N+1). DataLoader batches by key: one query loads all plans for the requested plan IDs, then each resolver gets its plan from the batch. That yields a bounded number of round-trips regardless of list size.

---

## Which loaders exist

Defined in `PayorClaims.Schema/Loaders/`:

| Loader | Key | Returns | Use |
|--------|-----|--------|-----|
| **CoveragesByMemberIdLoader** | MemberId | List of Coverages | Member.coverages, Member.activeCoverage |
| **PlanByIdLoader** | PlanId | Plan | Coverage.plan, activePlan |
| **EffectiveBenefitsByPlanIdLoader** | PlanId (+ asOf date) | List of PlanBenefit | Plan.effectiveBenefits |
| **ClaimLinesByClaimIdLoader** | ClaimId | List of ClaimLine | Claim.lines |
| **DiagnosesByClaimIdLoader** | ClaimId | List of ClaimDiagnosis | Claim.diagnoses |
| **ClaimsByMemberIdLoader** | MemberId | List of Claim | (verify in code) |
| **ProviderByIdLoader** | ProviderId | Provider | Claim.provider |

(Verify in code for any additional loaders or signature changes.)

---

## Rules for adding new loaders

1. **Batch by key:** The loader receives a set of keys (e.g. IDs) and returns a result per key. Use `IDataLoaderContextAccessor` and the same execution context so the same loader instance is used for the whole request.
2. **Return all keys:** The loader must return a value (or null) for every requested key so resolvers do not hang. Use a dictionary or list ordered by key.
3. **AsNoTracking:** Use `AsNoTracking()` for read-only loads to avoid EF change-tracking overhead.
4. **Single query:** Load all requested entities in one (or a small fixed number of) query; avoid per-key queries inside the loader.

Example pattern (conceptually): resolver calls `loader.LoadAsync(id, accessor.Context)`; the loader batches all `id` values for the current request and executes one query like `db.Entities.Where(e => keys.Contains(e.Id)).ToListAsync()`, then returns a lookup.

---

## How to prove N+1 is solved

1. **Enable EF SQL logging:** In Development, set logging level for `Microsoft.EntityFrameworkCore.Database.Command` to `Information` (e.g. in `appsettings.Development.json` or Serilog). Or use a simple diagnostic: `optionsBuilder.LogTo(Console.WriteLine)` in DbContext (temporarily).
2. **Run a Member360-style query:** Request a member with `activeCoverage { plan { effectiveBenefits(asOf: "...") { ... } } }` and `recentClaims(limit: 5) { lines { ... } diagnoses { ... } provider { ... } }`. This touches members, coverages, plans, benefits, claims, lines, diagnoses, providers.
3. **Expected:** A bounded number of queries (e.g. one or a few per entity type), not one per coverage, per claim, per line. Count the SQL statements in the log; they should not scale with the number of nested items (e.g. 5 claims × 1 query for all lines, 1 for all diagnoses, 1 for providers, not 5+5+5).
4. **If you see N+1:** Add or fix a DataLoader for the relation that is causing repeated queries (e.g. resolve the parent’s foreign key and load by that key in a batch).
