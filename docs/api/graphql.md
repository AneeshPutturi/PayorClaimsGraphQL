# GraphQL API

How to use the GraphQL endpoint, Altair, schema discovery, and common operations. Copy-paste-ready examples are included.

---

## Endpoints

| Method / transport | Path | Use |
|--------------------|------|-----|
| **POST** | `/graphql` | Queries and mutations. Body: `{ "query": "...", "operationName": null, "variables": {} }`. When **PersistedQueriesEnabled** is true, raw `query` is rejected; only `extensions.persistedQuery.sha256Hash` is accepted. |
| **GET** | `/graphql` | Schema introspection, persisted GET (if supported). Served by MapGraphQL. |
| **WebSocket** | `/graphql` | Subscriptions. Connect to `ws://localhost:5194/graphql` (or `wss://` with HTTPS). |

The custom POST handler in `PayorClaims.Api` (GraphQLHttpEndpoint) runs first for POST; GET and WebSocket are handled by `MapGraphQL<AppSchema>("/graphql")`.

---

## Schema discovery

- **Altair:** Open http://localhost:5194/ui/altair. Use the **Docs** panel to browse queries, mutations, subscriptions, and types.
- **Introspection:** Send an introspection query to `/graphql` (e.g. via Altair “Schema” or a tool that queries `__schema`).

---

## Important: ID arguments (GUID parsing)

GraphQL.NET’s **IdGraphType** exposes ID arguments as **strings** in resolvers. Resolvers must parse them to `Guid` (e.g. `Guid.TryParse(idStr, out var id)`). If they use `GetArgument<Guid>("id")` without parsing from string, the value can be `default(Guid)` and lookups return null. See [Troubleshooting – memberById returns null](../troubleshooting/common-issues.md).

---

## Copy-paste examples

### 1) Ping

```graphql
query {
  ping
}
```

### 2) Member 360 (member by ID with coverage and claims)

```graphql
query {
  memberById(id: "YOUR-MEMBER-GUID") {
    id
    firstName
    lastName
    status
    activeCoverage(asOf: "2026-02-07") {
      id
      plan { planCode name effectiveBenefits(asOf: "2026-02-07") { category network copayAmount } }
    }
    recentClaims(limit: 5) {
      claimNumber status
      provider { npi name }
      lines { lineNumber cptCode billedAmount lineStatus }
      diagnoses { codeSystem code isPrimary lineNumber }
    }
  }
}
```

### 3) submitClaim mutation (deep tree input)

```graphql
mutation {
  submitClaim(input: {
    memberId: "MEMBER-GUID"
    providerId: "PROVIDER-GUID"
    serviceFrom: "2026-01-01"
    serviceTo: "2026-01-01"
    receivedDate: "2026-01-15"
    idempotencyKey: "unique-key-123"
    diagnoses: [{ codeSystem: "ICD-10", code: "Z00.00", isPrimary: true }]
    lines: [
      {
        lineNumber: 1
        cptCode: "99213"
        units: 1
        billedAmount: 150.00
        diagnoses: [{ codeSystem: "ICD-10", code: "Z00.00", isPrimary: true }]
      }
    ]
  }) {
    claim { id claimNumber status }
    alreadyExisted
  }
}
```

### 4) adjudicateClaim mutation (rowVersion base64)

Get `rowVersion` from a previous claim query (`claim { rowVersion }`); it is returned as base64. Then:

```graphql
mutation {
  adjudicateClaim(
    claimId: "CLAIM-GUID"
    rowVersion: "BASE64_ROWVERSION_STRING"
    lines: [
      { lineNumber: 1, status: "Paid", allowedAmount: 120.00, paidAmount: 96.00 }
    ]
  ) {
    claim { id claimNumber status totalAllowed totalPaid }
  }
}
```

On concurrency conflict (row version mismatch), the API returns error code `CONCURRENCY_CONFLICT`. Refetch the claim and retry with the new `rowVersion`.

### 5) uploadClaimAttachment (base64, 5 MB cap)

```graphql
mutation {
  uploadClaimAttachment(
    claimId: "CLAIM-GUID"
    fileName: "note.pdf"
    contentType: "application/pdf"
    base64: "JVBERi0xLjQK..."
  ) {
    id claimId fileName storageKey uploadedAt
  }
}
```

Attachment size is limited to **5 MB** (enforced in `ClaimAttachmentService`). Invalid base64 returns “Invalid base64 content.”

### 6) Export: request + download link

Request:

```graphql
mutation {
  requestMemberClaimsExport(memberId: "MEMBER-GUID") {
    jobId
    status
    downloadTokenOnce
    expiresAt
  }
}
```

Use `downloadTokenOnce` once with the download endpoint (see [Exports](../ops/exports.md)):

```
GET /exports/{jobId}/download?token={downloadTokenOnce}
```

Header: `Authorization: Bearer <JWT>` (same user who requested the export, or Admin).

### 7) Subscriptions (WebSocket)

Connect to `ws://localhost:5194/graphql`, then:

**claimStatusChanged:**

```graphql
subscription {
  claimStatusChanged(claimId: "CLAIM-GUID") {
    id claimNumber status
  }
}
```

**eobGenerated:**

```graphql
subscription {
  eobGenerated(memberId: "MEMBER-GUID") {
    id claimId eobNumber generatedAt deliveryStatus
  }
}
```

---

## Error codes and responses

| Code / behavior | Meaning |
|-----------------|---------|
| **VALIDATION_FAILED** | Validation failed (e.g. submit/adjudicate). Response includes `errors[]` in extensions. |
| **CONCURRENCY_CONFLICT** | Optimistic concurrency failed (e.g. adjudicate with stale rowVersion). Refetch and retry. |
| **QUERY_TOO_DEEP** | Query depth exceeded `GraphQL:MaxDepth`. |
| **QUERY_TOO_COMPLEX** | Complexity exceeded `GraphQL:MaxComplexity`. |
| **PERSISTED_ONLY** | Persisted queries are enabled but request sent a raw `query` string. |
| **MISSING_HASH** | Persisted queries enabled but `extensions.persistedQuery.sha256Hash` missing. |
| **UNKNOWN_HASH** | Persisted queries enabled but the hash is not in the persisted-queries store. |
| **FORBIDDEN** | e.g. Admin-only mutation (registerWebhook, deactivateWebhook) called by non-Admin. |
| **UNAUTHORIZED** | Authentication required (e.g. requestMemberClaimsExport without token). |

Standard GraphQL errors return `{ "errors": [ { "message": "...", "code": "...", "extensions": { ... } } ] }`. The custom POST endpoint returns 400 with a JSON body for persisted-query and body validation failures.
