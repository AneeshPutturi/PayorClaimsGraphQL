# GraphQL API

How to use the PayorClaims GraphQL API: endpoints, schema discovery, and **ready-to-use examples** for queries, mutations, and subscriptions. Replace placeholder IDs and values with your own, then run them in Altair or any GraphQL client.

---

## Endpoints at a glance

| What | URL | Use |
|------|-----|-----|
| **GraphQL (HTTP)** | `http://localhost:5194/graphql` | Queries and mutations. Send `POST` with body `{ "query": "...", "variables": {} }`. |
| **Altair IDE** | `http://localhost:5194/ui/altair` | Browser-based client: write operations, set headers (e.g. `Authorization: Bearer <token>`), run and inspect results. |
| **Subscriptions (WebSocket)** | `ws://localhost:5194/graphql` | Long-lived connection for real-time events (claim status, EOB generated). |

When **PersistedQueriesEnabled** is `true`, POST only accepts `extensions.persistedQuery.sha256Hash` (no raw `query`). Keep it `false` in development so you can try the examples below.

---

## Schema discovery

In **Altair**, open the **Docs** panel (schema tab) to browse:

- **Queries** – `ping`, `memberById`, `claimById`, `members`, `claims`, `providers`, `providerByNpi`, `exportJob`
- **Mutations** – `submitClaim`, `adjudicateClaim`, `uploadClaimAttachment`, `registerWebhook`, `deactivateWebhook`, `requestMemberClaimsExport`, appeals
- **Subscriptions** – `claimStatusChanged`, `eobGenerated`

Use Docs to see exact field and argument types and to build your own operations.

---

## Important: ID arguments

GraphQL IDs are strings. Resolvers parse them to GUIDs. Always pass IDs as strings (e.g. `"a1b2c3d4-e5f6-7890-abcd-ef1234567890"`). If a by-id query returns `null`, check that the ID format is correct and that the entity exists. See [Troubleshooting – memberById returns null](../troubleshooting/common-issues.md).

---

# Query examples

Use these to read members, claims, providers, and export job status. No auth required for `ping`; other queries may require a JWT depending on your auth setup.

---

### Health check (no auth)

**When to use:** Verify the API is up and that GraphQL is responding.

```graphql
query {
  ping
}
```

**Returns:** `"pong"`.

---

### Single member by ID (Member 360 style)

**When to use:** Load one member with current coverage, plan benefits, and recent claims in one request. Good for a member detail or dashboard view.

```graphql
query Member360($memberId: ID!) {
  memberById(id: $memberId) {
    id
    firstName
    lastName
    status
    dob
    externalMemberNumber
    activeCoverage(asOf: "2026-02-07") {
      id
      coverageStatus
      startDate
      endDate
      plan {
        planCode
        name
        effectiveBenefits(asOf: "2026-02-07") {
          category
          network
          copayAmount
        }
      }
    }
    recentClaims(limit: 5) {
      claimNumber
      status
      totalBilled
      totalAllowed
      totalPaid
      receivedDate
      provider { npi name }
      lines { lineNumber cptCode billedAmount lineStatus }
      diagnoses { codeSystem code isPrimary lineNumber }
    }
  }
}
```

**Variables:** `{ "memberId": "your-member-guid-here" }`  
**Returns:** One member with nested coverage, plan, benefits, and up to 5 claims with lines and diagnoses.

---

### Member by ID (minimal fields)

**When to use:** Quick lookup when you only need basic info (e.g. after creating a member or for a dropdown).

```graphql
query {
  memberById(id: "your-member-guid") {
    id
    firstName
    lastName
    status
    externalMemberNumber
  }
}
```

---

### Paginated list of members (with filters and sort)

**When to use:** Member search or admin list: filter by status or name, sort by name or DOB, page through results.

```graphql
query MembersList($filter: MemberFilterInput, $page: PageInput, $sortField: MemberSortField, $sortDir: SortDirection) {
  members(filter: $filter, page: $page, sortField: $sortField, sortDir: $sortDir) {
    totalCount
    skip
    take
    items {
      id
      firstName
      lastName
      status
      dob
      externalMemberNumber
    }
  }
}
```

**Variables (first page, active only, sort by last name):**
```json
{
  "filter": { "status": "Active" },
  "page": { "skip": 0, "take": 20 },
  "sortField": "LastName",
  "sortDir": "ASC"
}
```

**Variables (search by name):**
```json
{
  "filter": { "nameContains": "Smith" },
  "page": { "skip": 0, "take": 10 },
  "sortField": "LastName",
  "sortDir": "ASC"
}
```

**Returns:** `totalCount`, `skip`, `take`, and `items` array. Use `skip`/`take` for pagination (e.g. second page: `skip: 20, take: 20`).

---

### Single claim by ID (with lines and provider)

**When to use:** Claim detail page or adjudication workflow: full claim, lines, diagnoses, and provider.

```graphql
query {
  claimById(id: "your-claim-guid") {
    id
    claimNumber
    status
    totalBilled
    totalAllowed
    totalPaid
    receivedDate
    serviceFromDate
    serviceToDate
    rowVersion
    memberId
    providerId
    provider { id npi name providerStatus }
    lines { lineNumber cptCode units billedAmount allowedAmount paidAmount lineStatus }
    diagnoses { codeSystem code isPrimary lineNumber }
  }
}
```

**Returns:** One claim. Use `rowVersion` in the **adjudicateClaim** mutation for optimistic concurrency.

---

### Paginated list of claims (filter and sort)

**When to use:** Claim queue, reports, or “all claims for a member/provider”. Filter by member, provider, status, date range, or claim number.

```graphql
query ClaimsList($filter: ClaimFilterInput, $page: PageInput, $sortField: ClaimSortField, $sortDir: SortDirection) {
  claims(filter: $filter, page: $page, sortField: $sortField, sortDir: $sortDir) {
    totalCount
    skip
    take
    items {
      id
      claimNumber
      status
      totalBilled
      receivedDate
      memberId
      providerId
    }
  }
}
```

**Variables (claims for one member, newest first):**
```json
{
  "filter": { "memberId": "member-guid-here" },
  "page": { "skip": 0, "take": 25 },
  "sortField": "ReceivedDate",
  "sortDir": "DESC"
}
```

**Variables (pending claims in a date range):**
```json
{
  "filter": {
    "status": "Pending",
    "receivedFrom": "2026-01-01",
    "receivedTo": "2026-01-31"
  },
  "page": { "skip": 0, "take": 50 },
  "sortField": "ReceivedDate",
  "sortDir": "DESC"
}
```

---

### Provider by NPI

**When to use:** Look up a provider by NPI (e.g. when building a claim or validating a submitter).

```graphql
query {
  providerByNpi(npi: "1234567890") {
    id
    npi
    name
    providerType
    providerStatus
    specialty
  }
}
```

---

### Paginated list of providers

**When to use:** Provider directory or search: filter by NPI, name, specialty, or status; sort by name or NPI.

```graphql
query ProvidersList($filter: ProviderFilterInput, $page: PageInput, $sortField: ProviderSortField, $sortDir: SortDirection) {
  providers(filter: $filter, page: $page, sortField: $sortField, sortDir: $sortDir) {
    totalCount
    skip
    take
    items {
      id
      npi
      name
      providerType
      providerStatus
      specialty
    }
  }
}
```

**Variables (active providers, name contains “Medical”):**
```json
{
  "filter": { "status": "Active", "nameContains": "Medical" },
  "page": { "skip": 0, "take": 20 },
  "sortField": "Name",
  "sortDir": "ASC"
}
```

---

### Export job status (after requesting an export)

**When to use:** After calling `requestMemberClaimsExport`, poll for status and get the one-time download token when ready.

```graphql
query {
  exportJob(jobId: "your-export-job-guid") {
    status
    readyAt
    downloadTokenOnce
    expiresAt
  }
}
```

**Returns:** `status` (e.g. Queued, Running, Ready, Failed), and when Ready: `downloadTokenOnce` and `expiresAt`. Use the token with `GET /exports/{jobId}/download?token={downloadTokenOnce}` (see [Exports](../ops/exports.md)).

---

### EOB by ID

**When to use:** Fetch a single EOB (e.g. after an `eobGenerated` subscription event).

```graphql
query {
  eobById(id: "your-eob-guid") {
    id
    claimId
    eobNumber
    generatedAt
    documentStorageKey
    deliveryStatus
  }
}
```

---

# Mutation examples

Mutations change data. Most require authentication (JWT). Send a `mutation` operation in the same way as queries (POST to `/graphql`). Use variables for cleaner, reusable requests.

---

### Submit a claim (single line, single diagnosis)

**When to use:** Provider or system submits a new claim. Use a unique `idempotencyKey` per logical submission to avoid duplicates.

```graphql
mutation SubmitClaim($input: ClaimSubmissionInput!) {
  submitClaim(input: $input) {
    claim { id claimNumber status totalBilled receivedDate }
    alreadyExisted
  }
}
```

**Variables (minimal):**
```json
{
  "input": {
    "memberId": "member-guid",
    "providerId": "provider-guid",
    "serviceFrom": "2026-01-01",
    "serviceTo": "2026-01-01",
    "receivedDate": "2026-01-15",
    "idempotencyKey": "claim-2026-01-15-prov-123-mem-456",
    "diagnoses": [
      { "codeSystem": "ICD-10", "code": "Z00.00", "isPrimary": true }
    ],
    "lines": [
      {
        "lineNumber": 1,
        "cptCode": "99213",
        "units": 1,
        "billedAmount": 150.00,
        "diagnoses": [
          { "codeSystem": "ICD-10", "code": "Z00.00", "isPrimary": true, "lineNumber": 1 }
        ]
      }
    ]
  }
}
```

**Returns:** `claim` (with id, claimNumber, status, etc.) and `alreadyExisted` (true if the same idempotency key was already processed).

---

### Submit a claim (multiple lines)

**When to use:** Multi-line claim (e.g. office visit + lab). Same shape as above; add more entries to `lines` and optionally more claim-level `diagnoses`.

```graphql
mutation SubmitMultiLineClaim($input: ClaimSubmissionInput!) {
  submitClaim(input: $input) {
    claim { id claimNumber status }
    alreadyExisted
  }
}
```

**Variables (two lines):**
```json
{
  "input": {
    "memberId": "member-guid",
    "providerId": "provider-guid",
    "serviceFrom": "2026-01-10",
    "serviceTo": "2026-01-10",
    "receivedDate": "2026-01-20",
    "idempotencyKey": "claim-multi-2026-01-20-001",
    "diagnoses": [
      { "codeSystem": "ICD-10", "code": "J06.9", "isPrimary": true }
    ],
    "lines": [
      {
        "lineNumber": 1,
        "cptCode": "99213",
        "units": 1,
        "billedAmount": 125.00,
        "diagnoses": [{ "codeSystem": "ICD-10", "code": "J06.9", "isPrimary": true, "lineNumber": 1 }]
      },
      {
        "lineNumber": 2,
        "cptCode": "80053",
        "units": 1,
        "billedAmount": 25.00,
        "diagnoses": [{ "codeSystem": "ICD-10", "code": "J06.9", "isPrimary": false, "lineNumber": 2 }]
      }
    ]
  }
}
```

---

### Adjudicate a claim (approve/deny lines)

**When to use:** Adjuster sets line-level allowed/paid amounts and status. You must pass the claim’s current `rowVersion` (from a prior `claimById` query) for optimistic concurrency.

```graphql
mutation AdjudicateClaim($claimId: ID!, $rowVersion: String!, $lines: [AdjudicateLineInput!]!) {
  adjudicateClaim(claimId: $claimId, rowVersion: $rowVersion, lines: $lines) {
    claim { id claimNumber status totalAllowed totalPaid lines { lineNumber status allowedAmount paidAmount } }
  }
}
```

**Variables (pay one line, deny another):**
```json
{
  "claimId": "claim-guid-from-query",
  "rowVersion": "base64-rowversion-from-claimById",
  "lines": [
    { "lineNumber": 1, "status": "Paid", "allowedAmount": 120.00, "paidAmount": 96.00 },
    { "lineNumber": 2, "status": "Denied", "denialReasonCode": "N261", "allowedAmount": 0, "paidAmount": 0 }
  ]
}
```

**Returns:** Updated `claim`. If another user adjudicated first, you get error code `CONCURRENCY_CONFLICT`; refetch the claim and retry with the new `rowVersion`.

---

### Upload a claim attachment (base64)

**When to use:** Attach a file (e.g. PDF, image) to a claim. Max size **5 MB**. Content is sent as base64.

```graphql
mutation UploadAttachment($claimId: ID!, $fileName: String!, $contentType: String!, $base64: String!) {
  uploadClaimAttachment(claimId: $claimId, fileName: $fileName, contentType: $contentType, base64: $base64) {
    id
    claimId
    fileName
    contentType
    storageKey
    sha256
    uploadedAt
  }
}
```

**Variables (example; replace base64 with real content):**
```json
{
  "claimId": "claim-guid",
  "fileName": "referral.pdf",
  "contentType": "application/pdf",
  "base64": "JVBERi0xLjQKJeLjz9MK..."
}
```

**Returns:** The created attachment (id, claimId, fileName, storageKey, etc.). Invalid base64 or size > 5 MB returns an error.

---

### Request a member claims export

**When to use:** User (or Admin) requests an export of all claims for a member. Returns a job id and, when ready, a one-time download token.

```graphql
mutation RequestExport($memberId: ID!) {
  requestMemberClaimsExport(memberId: $memberId) {
    jobId
    status
    downloadTokenOnce
    expiresAt
  }
}
```

**Variables:** `{ "memberId": "member-guid" }`  
**Auth:** Required (Bearer token). Typically the same member (self-export) or Admin.  
**Returns:** `jobId` (use with `exportJob` query and with `/exports/{jobId}/download?token=...` when status is Ready).

---

### Register a webhook (Admin only)

**When to use:** Register a URL to receive events (e.g. claim status changed, EOB generated). Optional secret for signature verification.

```graphql
mutation RegisterWebhook($name: String!, $url: String!, $secret: String) {
  registerWebhook(name: $name, url: $url, secret: $secret) {
    id
    name
    url
    isActive
    createdAt
  }
}
```

**Variables:**
```json
{
  "name": "My integration",
  "url": "https://my-server.com/webhooks/payor",
  "secret": "optional-shared-secret-for-hmac"
}
```

**Returns:** The created webhook endpoint. Requires **Admin** role.

---

### Deactivate a webhook (Admin only)

**When to use:** Stop deliveries to an endpoint without deleting it.

```graphql
mutation DeactivateWebhook($id: ID!) {
  deactivateWebhook(id: $id) {
    id
    name
    url
    isActive
  }
}
```

**Variables:** `{ "id": "webhook-endpoint-guid" }`  
**Returns:** The endpoint with `isActive: false`. Requires **Admin** role.

---

### Submit an appeal (on a claim)

**When to use:** Provider or member submits an appeal for a denied or partially paid claim.

```graphql
mutation SubmitAppeal($claimId: ID!, $level: Int!, $reason: String!) {
  submitAppeal(claimId: $claimId, level: $level, reason: $reason) {
    id
    claimId
    appealLevel
    reason
    status
    submittedAt
  }
}
```

**Variables:** `{ "claimId": "claim-guid", "level": 1, "reason": "Medical necessity supported by documentation." }`  
**Returns:** The created appeal record.

---

# Subscription examples

Subscriptions use a **WebSocket** connection to `ws://localhost:5194/graphql`. In Altair, switch the endpoint to the WebSocket URL and send a subscription; you’ll receive a stream of events. Replace placeholder IDs with real claim or member IDs.

---

### Listen for claim status changes

**When to use:** Real-time UI or integration when a specific claim is adjudicated, appealed, or updated. Pass the claim ID you care about.

```graphql
subscription OnClaimStatusChanged($claimId: ID!) {
  claimStatusChanged(claimId: $claimId) {
    id
    claimNumber
    status
    totalAllowed
    totalPaid
    receivedDate
  }
}
```

**Variables:** `{ "claimId": "claim-guid" }`  
**Returns:** A stream of `Claim` objects whenever that claim’s status changes (e.g. after adjudication or appeal decision). Use for live dashboards or downstream systems.

---

### Listen for EOBs for a member

**When to use:** Notify when an EOB is generated for any claim belonging to a member (e.g. “your EOB is ready”).

```graphql
subscription OnEobGenerated($memberId: ID!) {
  eobGenerated(memberId: $memberId) {
    id
    claimId
    eobNumber
    generatedAt
    documentStorageKey
    deliveryStatus
  }
}
```

**Variables:** `{ "memberId": "member-guid" }`  
**Returns:** A stream of `Eob` objects for that member. Filter client-side by member if needed; the server already filters by `memberId`.

---

### Using both subscriptions (two operations)

You can run multiple subscriptions over the same WebSocket (e.g. one tab for claim updates, one for EOBs). Example for a single claim and a single member:

```graphql
subscription ClaimAndEob($claimId: ID!, $memberId: ID!) {
  claimStatusChanged(claimId: $claimId) {
    id claimNumber status totalPaid
  }
  eobGenerated(memberId: $memberId) {
    id eobNumber generatedAt
  }
}
```

**Variables:** `{ "claimId": "claim-guid", "memberId": "member-guid" }`  
**Note:** In GraphQL you subscribe to one root field per subscription document; the exact shape depends on your client. Use two separate subscription documents if your client does not support multiple root fields in one subscription.

---

# Error codes and responses

| Code | Meaning | What to do |
|------|---------|------------|
| **VALIDATION_FAILED** | Input validation failed (e.g. submit/adjudicate). | Check `errors` in the response extensions; fix input and retry. |
| **CONCURRENCY_CONFLICT** | Optimistic concurrency failed (stale rowVersion). | Refetch the claim, get new `rowVersion`, retry adjudication. |
| **QUERY_TOO_DEEP** | Query nesting exceeded `GraphQL:MaxDepth`. | Simplify the query or increase MaxDepth in config. |
| **QUERY_TOO_COMPLEX** | Complexity exceeded `GraphQL:MaxComplexity`. | Reduce list sizes or fields, or increase MaxComplexity. |
| **PERSISTED_ONLY** | Persisted queries are on but request sent raw `query`. | Use a persisted query hash or set PersistedQueriesEnabled to false in dev. |
| **MISSING_HASH** | Persisted queries on but no `extensions.persistedQuery.sha256Hash`. | Send the hash in extensions. |
| **UNKNOWN_HASH** | Hash not found in the persisted-queries store. | Add the query to persisted-queries.json or fix the hash. |
| **FORBIDDEN** | Not allowed (e.g. Admin-only mutation). | Use a token with the required role. |
| **UNAUTHORIZED** | Authentication required. | Send a valid `Authorization: Bearer <token>` header. |

Errors are returned in the standard GraphQL form: `{ "errors": [ { "message": "...", "code": "...", "extensions": { ... } } ] }`. The custom POST endpoint may return 400 with a JSON body for bad request or persisted-query violations.
