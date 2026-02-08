# HIPAA access logs and audit chain

This document describes the HIPAA-style access logging and the append-only audit event chain, including immutability guarantees and the validation job.

---

## HIPAA access logs

**Purpose:** Record access to PHI for compliance and accountability. Each read/write of sensitive entities can be logged with actor, subject, action, and purpose.

**Table:** `hipaa_access_logs` (see [Tables reference](../database/tables.md)).

**Behavior:**

- **Append-only:** Rows must never be updated or deleted. `ClaimsDbContext.EnforceHipaaAccessLogAppendOnly()` throws if any `HipaaAccessLog` entry is Modified or Deleted in SaveChanges.
- **Hash chain:** Each row has `PrevHash` and `Hash`. The hash is computed over a payload that includes the previous row’s hash so that tampering or reordering can be detected. (Verify in code: exact payload format for HIPAA log hashing; see `ClaimsDbContext` or related code.)
- **When logged:** GraphQL resolvers (and other call paths) call `IAccessLogger.LogReadAsync` / similar after loading sensitive entities. The implementation (`AccessLogger` in Infrastructure) writes to `HipaaAccessLog`. (Verify in code: which operations are logged and payload format.)

**Safety / compliance note:** This is a demo pattern. Production systems typically integrate with dedicated audit/SIEM solutions and enforce retention and access policies. Ensure logging covers all required access and that logs are protected and retained per policy.

---

## Audit events (append-only hash chain)

**Purpose:** Immutable audit trail of significant actions (e.g. claim submitted, claim adjudicated). Used for accountability and dispute resolution.

**Table:** `audit_events` (see [Tables reference](../database/tables.md)).

**Behavior:**

- **Append-only:** No updates or deletes. `ClaimsDbContext.EnforceAuditEventAppendOnly()` throws if any `AuditEvent` is Modified or Deleted.
- **Hash chain:** On insert, `ClaimsDbContext.ComputeAuditEventHashChain()` sets `PrevHash` to the previous event’s `Hash` and computes `Hash` over a payload: `PrevHash|ActorUserId|Action|EntityType|EntityId|OccurredAt|DiffJson`. Hash is SHA256 hex. This links events in order; any change or gap breaks the chain.
- **When written:** During SaveChanges when new `AuditEvent` entities are added (typically from application code or services that record actions).

---

## Audit chain validation job

**Purpose:** Detect tampering or data corruption in the audit event chain.

**Implementation:** `PayorClaims.Infrastructure.Audit.AuditChainValidationJob` (background service).

**Schedule:** Runs every **1 hour** (interval configurable in code; verify `AuditChainValidationJob.Interval`).

**Logic:**

1. Load up to **10,000** audit events in order (`OccurredAt`, `Id`).
2. For each event, verify:
   - `PrevHash` equals the previous event’s `Hash` (or empty for the first).
   - Recompute `Hash` from the same payload; it must match the stored `Hash`.
3. If any check fails: log at **Critical** and stop (chain integrity broken or tampering detected).

**Safety / compliance note:** The job only validates; it does not correct data. Alerts and incident response should be defined when validation fails. For production, consider validating the full chain or sampling and retaining validation results.

---

## Where enforcement lives

| Concern | Location |
|--------|----------|
| HipaaAccessLog append-only | `ClaimsDbContext.EnforceHipaaAccessLogAppendOnly()` |
| AuditEvent append-only | `ClaimsDbContext.EnforceAuditEventAppendOnly()` |
| AuditEvent hash chain computation | `ClaimsDbContext.ComputeAuditEventHashChain()` |
| Audit chain validation | `AuditChainValidationJob.ValidateChainAsync()` |

All run within the same process; the database is the source of truth. For HIPAA log hashing, check `HipaaAccessLog` entity and any hash computation in Infrastructure (verify in code if not in DbContext).
