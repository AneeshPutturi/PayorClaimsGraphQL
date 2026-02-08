# Export jobs

Export job lifecycle, secure one-time download tokens, and how to use the download endpoint.

---

## Job lifecycle

| Status | Meaning |
|--------|---------|
| **Queued** | Job created; waiting for worker to pick it up. |
| **Running** | Worker is generating the export file. |
| **Ready** | File written; download token and expiry set. Client can download once with the token. |
| **Failed** | Export generation failed (verify in code for error storage). |

**Flow:**

1. Client calls **requestMemberClaimsExport(memberId)** (GraphQL mutation). Requires authenticated user (Admin or the member for self). `ExportService` creates an `ExportJob` with `Status = 'Queued'`.
2. **ExportJobWorker** runs every **10 seconds**. It picks one `Queued` job, sets `Running`, writes JSON to `Storage:LocalPath/exports/{jobId}.json`, sets `FilePath`, generates a one-time token, stores its hash in `DownloadTokenHash`, sets `ExpiresAt`, sets `Status = 'Ready'` (or `Failed` on error). (Verify in code: exact token generation and expiry duration.)
3. The mutation response (or a follow-up **exportJob(jobId)** query) returns `downloadTokenOnce` and `expiresAt`. Client uses the token once with the download endpoint.

---

## One-time token and expiry

- The token is a cryptographically random value returned only once in the API response (`downloadTokenOnce`). It is stored as **SHA256 hash** in `ExportJob.DownloadTokenHash`.
- **ExpiresAt:** After this time the download endpoint returns Unauthorized (“Export link expired”). (Verify in code for default expiry, e.g. 24 hours.)
- **One-time:** The API does not invalidate the token after first use; best practice is to treat it as single-use and not reuse. (Verify in code if a “consumed” flag exists.)

---

## Download endpoint

**URL:** `GET /exports/{jobId}/download?token={downloadTokenOnce}`

**Authentication:** Request must be **authenticated** (Bearer JWT). The user must be either the same actor who requested the export (`RequestedByActorId` / `RequestedByActorType`) or Admin.

**Behavior:**

1. **Missing token:** 400 Bad Request.
2. **Job not found:** 404.
3. **Token mismatch:** Compute SHA256 of the provided token (hex); compare to `job.DownloadTokenHash`. If different → 401 Unauthorized (“Invalid token”).
4. **Expired:** If `job.ExpiresAt` is null or in the past → 401 (“Export link expired”).
5. **Not ready:** If `job.Status != 'Ready'` or `job.FilePath` is null → 404 (“Export not ready”).
6. **Authorization:** If the authenticated actor is not Admin and (actor id/type does not match job’s requested-by) → 403 Forbid.
7. **File missing:** If the file at `job.FilePath` does not exist on disk → 404 (“File no longer available”).
8. **Success:** Return file with `PhysicalFile(..., "application/json", fileName, ...)`. Filename is of the form `claims-export-{memberId}.json`.

**Controller:** `PayorClaims.Api.Controllers.ExportDownloadController`.

---

## Query limits (GraphQL)

**MaxDepth** and **MaxComplexity** (config keys under `GraphQL`) limit how deep and how expensive a query can be to reduce DoS and accidental heavy queries.

- **MaxDepth** (default 12): Maximum depth of the query document. Exceeding it returns error code **QUERY_TOO_DEEP**.
- **MaxComplexity** (default 2000): Complexity is computed by the complexity analyzer (see Schema registration). Exceeding it returns **QUERY_TOO_COMPLEX**.

Configure in `appsettings.json` or environment. See [Configuration](../setup/configuration.md).

---

## Persisted queries

When **GraphQL:PersistedQueriesEnabled** is `true`, the **HTTP POST** `/graphql` endpoint (custom handler) **only** accepts requests that provide a persisted query hash; raw `query` string is rejected with 400 **PERSISTED_ONLY**.

**How to add entries:** Edit the file at **GraphQL:PersistedQueriesPath** (default `persisted-queries.json`). Format is a JSON object: keys = SHA256 hash (hex, lowercase) of the query document (no whitespace normalization; use the exact string the client will send), values = the query string. Example:

```json
{
  "a1b2c3d4e5...": "query { memberById(id: \"...\") { id firstName } }"
}
```

Compute SHA256 of the query string (as sent) to get the key. The client sends `extensions: { persistedQuery: { sha256Hash: "<key>" } }` and no `query` field.

**Production vs dev:** In Production you can enable persisted queries for security and performance. In Development keep **PersistedQueriesEnabled: false** so Altair and other tools can send ad-hoc queries. GET and WebSocket are not enforced by the custom handler; only POST is.

**HTTP enforcement details:** The custom endpoint (GraphQLHttpEndpoint) checks `PersistedQueriesEnabled`. If true: reject when `query` is non-empty; require `extensions.persistedQuery.sha256Hash`; look up query text via `IPersistedQueryStore.GetQueryByHashAsync`; if missing return 400 **UNKNOWN_HASH**. If false, use `request.Query` as usual.

---

## Querying job status

Use the **exportJob(jobId)** GraphQL query to get status, `downloadTokenOnce` (if still available), and `expiresAt`. Only the requesting actor or Admin should have access (enforced in `IExportService.GetExportJobStatusAsync`; verify in code).
