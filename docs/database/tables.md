# Tables reference

Table-by-table reference: purpose, key columns, relations, and constraints. Source: `PayorClaims.Infrastructure` (entities, configurations, migrations). Column types are logical (e.g. GUID, string, date); exact SQL types are in migrations.

---

## Core

### members

**Purpose:** Enrollees (members). Soft-deleted via `IsDeleted`. Optional `SsnPlain` for dev/test; production may use encrypted SSN only.

| Column | Type | Notes |
|--------|------|--------|
| Id | GUID | PK |
| ExternalMemberNumber | string(30) | Required |
| FirstName, LastName | string(100) | Required |
| Dob | date | |
| Gender | string(20) | Nullable |
| Status | string(20) | Required |
| EmailEncrypted, PhoneEncrypted, SsnEncrypted | binary | Nullable |
| SsnPlain | string(20) | Nullable; used when not encrypting (e.g. tests) |
| CreatedAt, UpdatedAt, IsDeleted, DeletedAt, RowVersion | BaseEntity | |

**Relations:** One-to-many to coverages, claims, member_consents, accumulators, export_jobs.

---

### plans

**Purpose:** Benefit plans (plan code, name, year, network, metal tier).

| Column | Type | Notes |
|--------|------|--------|
| Id | GUID | PK |
| PlanCode | string(30) | Required |
| Name | string(200) | Required |
| Year | int | |
| NetworkType, MetalTier | string | Required |
| IsActive | bool | |
| CreatedAt, UpdatedAt, IsDeleted, DeletedAt, RowVersion | BaseEntity | |

**Relations:** One-to-many to coverages, plan_benefits, accumulators.

---

### coverages

**Purpose:** Member’s enrollment in a plan for a date range.

| Column | Type | Notes |
|--------|------|--------|
| Id | GUID | PK |
| MemberId, PlanId | GUID | FK |
| StartDate, EndDate | date | EndDate nullable for ongoing |
| CoverageStatus | string(20) | Required |
| CreatedAt, UpdatedAt, IsDeleted, DeletedAt, RowVersion | BaseEntity | |

**Relations:** Many-to-one members, plans; one-to-many claims (Claim.CoverageId).

---

### providers

**Purpose:** Healthcare providers (NPI, name, type, status).

| Column | Type | Notes |
|--------|------|--------|
| Id | GUID | PK |
| Npi | string(10) | Required |
| Name | string(200) | Required |
| ProviderType | string(30) | Required |
| Specialty, TaxId | string | Nullable |
| ProviderStatus | string(20) | Required |
| CreatedAt, UpdatedAt, IsDeleted, DeletedAt, RowVersion | BaseEntity | |

**Relations:** One-to-many provider_locations, claims.

---

### provider_locations

**Purpose:** Physical locations of a provider.

| Column | Type | Notes |
|--------|------|--------|
| Id | GUID | PK |
| ProviderId | GUID | FK |
| AddressLine1, AddressLine2 | string(200) | AddressLine2 nullable |
| City, State, Zip | string | Required |
| Phone | string(30) | Nullable |
| CreatedAt, UpdatedAt, IsDeleted, DeletedAt, RowVersion | BaseEntity | |

**Relations:** Many-to-one providers.

---

## Reference / lookup

### cpt_codes

**Purpose:** CPT procedure codes (code, description, effective range). (Verify in code: table name and whether BaseEntity.)

| Column | Type | Notes |
|--------|------|--------|
| Id | GUID | PK |
| CptCodeId, Description | string | (verify in code) |
| EffectiveFrom, EffectiveTo | date | Nullable |

---

### diagnosis_codes

**Purpose:** Diagnosis codes (code system, code, description, effective range). (Verify in code: exact column names.)

| Column | Type | Notes |
|--------|------|--------|
| Id | GUID | PK |
| CodeSystem, Code | string | |
| Description | string(500) | Required |
| EffectiveFrom, EffectiveTo | date | Nullable |

---

## Claims

### claims

**Purpose:** Medical claims (member, provider, coverage, service dates, amounts, idempotency, duplicate fingerprint).

| Column | Type | Notes |
|--------|------|--------|
| Id | GUID | PK |
| ClaimNumber | string(30) | Required |
| MemberId, ProviderId | GUID | FK, required |
| CoverageId | GUID | FK, nullable |
| ServiceFromDate, ServiceToDate, ReceivedDate | date | |
| Status | string(30) | Required |
| TotalBilled, TotalAllowed, TotalPaid | decimal | |
| Currency | string(3) | Required |
| IdempotencyKey | string(80) | Nullable |
| DuplicateFingerprint | string(64) | Required |
| SourceSystem | string(50) | Nullable |
| SubmittedByActorType | string(20) | Required |
| SubmittedByActorId | GUID? | Nullable |
| RowVersion | rowversion / byte[] | Concurrency |
| CreatedAt, UpdatedAt, IsDeleted, DeletedAt | BaseEntity | (RowVersion separate) |

**Relations:** Many-to-one members, providers, coverages; one-to-many claim_lines, claim_diagnoses, payments, eobs, claim_attachments, claim_appeals.

---

### claim_lines

**Purpose:** Line-level billing (CPT, units, amounts, status).

| Column | Type | Notes |
|--------|------|--------|
| Id | GUID | PK |
| ClaimId | GUID | FK |
| LineNumber | int | |
| CptCode | string(10) | Required |
| Units | int | |
| BilledAmount, AllowedAmount, PaidAmount | decimal | |
| LineStatus | string(30) | Required |
| DenialReasonCode, DenialReasonText | string | Nullable |
| CreatedAt, UpdatedAt, IsDeleted, DeletedAt, RowVersion | BaseEntity | |

**Relations:** Many-to-one claims.

---

### claim_diagnoses

**Purpose:** Diagnosis codes linked to a claim (and optionally a line). (Verify in code: no BaseEntity / soft delete.)

| Column | Type | Notes |
|--------|------|--------|
| Id | GUID | PK |
| ClaimId | GUID | FK |
| LineNumber | int | Nullable |
| CodeSystem, Code | string | Required |
| IsPrimary | bool | |

---

### payments

**Purpose:** Payments against a claim (amount, method, idempotency key).

| Column | Type | Notes |
|--------|------|--------|
| Id | GUID | PK |
| ClaimId | GUID | FK |
| Amount | decimal | |
| Method | string(20) | Required |
| ReferenceNumber, IdempotencyKey | string | Nullable |
| CreatedAt | datetime | (verify in code) |

---

### eobs

**Purpose:** Explanation of benefits (one per claim when generated). Document storage key and delivery status.

| Column | Type | Notes |
|--------|------|--------|
| Id | GUID | PK |
| ClaimId | GUID | FK |
| EobNumber | string(30) | Required |
| DocumentStorageKey | string(500) | Required |
| DocumentSha256 | string(64) | Required |
| DeliveryMethod, DeliveryStatus | string(20) | Required |
| FailureReason | string(500) | Nullable |
| GeneratedAt | datetime | (verify in code) |

**Relations:** Many-to-one claims.

---

## Benefits and consent

### accumulators

**Purpose:** Deductible/MOOP met per member/plan/year/network.

| Column | Type | Notes |
|--------|------|--------|
| Id | GUID | PK |
| MemberId, PlanId | GUID | FK |
| Year | int | Benefit year |
| Network | string(20) | Required |
| DeductibleMet, MoopMet | decimal | |
| CreatedAt, UpdatedAt, IsDeleted, DeletedAt, RowVersion | BaseEntity | |

**Relations:** Many-to-one members, plans. Unique index (MemberId, PlanId, Year, Network) (verify in code).

---

### member_consents

**Purpose:** Member consent records (type, granted, granted/revoked at).

| Column | Type | Notes |
|--------|------|--------|
| Id | GUID | PK |
| MemberId | GUID | FK |
| ConsentType | string(50) | Required (e.g. ProviderAccessPHI, EmailContact, PhoneContact) |
| Granted | bool | |
| GrantedAt | datetime | |
| RevokedAt | datetime? | Nullable |
| Source | string(50) | Required |
| CreatedAt, UpdatedAt, IsDeleted, DeletedAt, RowVersion | BaseEntity | |

**Relations:** Many-to-one members.

---

## Audit and HIPAA

### hipaa_access_logs

**Purpose:** Append-only access log for HIPAA-relevant access. Hash chain (PrevHash, Hash) for integrity.

| Column | Type | Notes |
|--------|------|--------|
| Id | GUID | PK |
| ActorType | string(20) | Required |
| ActorId | string? | (verify in code) |
| Action | string(30) | Required |
| SubjectType | string(30) | Required |
| SubjectId | GUID? | (verify in code) |
| PurposeOfUse | string(50) | Required |
| IpAddress, UserAgent | string | Nullable |
| PrevHash, Hash | string(64) | Required |
| OccurredAt | datetime | (verify in code) |

**Constraints:** Append-only (no updates/deletes); enforced in `ClaimsDbContext.EnforceHipaaAccessLogAppendOnly`.

---

### audit_events

**Purpose:** Append-only audit trail with hash chain. Validated hourly by `AuditChainValidationJob`.

| Column | Type | Notes |
|--------|------|--------|
| Id | GUID | PK |
| ActorUserId | string(100) | Required |
| Action | string(50) | Required |
| EntityType | string(50) | Required |
| EntityId | GUID | |
| OccurredAt | datetime | |
| DiffJson | string | Nullable |
| PrevHash, Hash | string(64) | Required |
| Notes | string(500) | Nullable |

**Constraints:** Append-only; enforced in `ClaimsDbContext.EnforceAuditEventAppendOnly`.

---

## Webhooks and export

### webhook_endpoints

**Purpose:** Registered webhook URLs (name, URL, secret, active).

| Column | Type | Notes |
|--------|------|--------|
| Id | GUID | PK |
| Name | string(200) | Required |
| Url | string(2000) | Required |
| Secret | string(500) | Nullable |
| IsActive | bool | |
| CreatedAt | datetime | (verify in code) |

**Relations:** One-to-many webhook_deliveries.

---

### webhook_deliveries

**Purpose:** Queue of webhook payloads to send (event type, payload, attempt count, next attempt, status).

| Column | Type | Notes |
|--------|------|--------|
| Id | GUID | PK |
| WebhookEndpointId | GUID | FK |
| EventType | string(100) | Required |
| PayloadJson | string | Required |
| Status | string(20) | Required (e.g. Pending, Succeeded, Failed) |
| AttemptCount | int | |
| NextAttemptAt, LastAttemptAt | datetime? | Nullable |
| LastStatusCode | int? | Nullable |
| LastError | string(2000) | Nullable |
| CreatedAt | datetime | (verify in code) |

**Relations:** Many-to-one webhook_endpoints.

---

### export_jobs

**Purpose:** Member claims export requests (status, file path, one-time download token hash, expiry).

| Column | Type | Notes |
|--------|------|--------|
| Id | GUID | PK |
| RequestedByActorType | string(50) | Required |
| RequestedByActorId | GUID? | Nullable |
| MemberId | GUID | FK |
| Status | string(20) | Required (Queued, Running, Ready, Failed) |
| FilePath | string(1000) | Nullable |
| DownloadTokenHash | string(64) | Nullable |
| ExpiresAt | datetime? | Nullable |
| CreatedAt, CompletedAt | datetime | (verify in code) |

**Relations:** Many-to-one members. Download via `GET /exports/{jobId}/download?token=...` using the one-time token (hash compared to DownloadTokenHash).
