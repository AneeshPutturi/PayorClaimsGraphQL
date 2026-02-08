# Configuration

All configuration keys used by the PayorClaims API, with examples. Sources: `appsettings.json`, `appsettings.Development.json`, and environment-specific overrides (e.g. test in-memory config).

---

## Configuration keys reference

| Section | Key | Type | Example / default | Description |
|---------|-----|------|-------------------|-------------|
| **ConnectionStrings** | ClaimsDb | string | `Server=localhost;Database=PayorClaimsGraphQL;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True` | EF Core connection for `ClaimsDbContext`. In **Testing** env, replaced by SQLite in-memory (see [Testing](../testing/testing-strategy.md)). |
| **Seed** | Enabled | bool | `true` | If true, on startup the app runs migrations (or EnsureCreated in Testing) and invokes `ISeedRunner.RunAsync`. |
| **Seed** | Reset | bool | `false` | (verify in code) Intended for seed reset behavior; see `SeedRunner` in Infrastructure. |
| **Storage** | Provider | string | `"Local"` | Storage provider; local disk is used when Local. |
| **Storage** | LocalPath | string | `"App_Data/uploads"` | Root for local storage. Subfolders: attachments, eobs, exports (verify in `ExportJobWorker` / `ClaimAttachmentService`). |
| **Auth** | Issuer | string | `"payorclaims"` | JWT `iss` claim; must match token issuer. |
| **Auth** | Audience | string | `"payorclaims"` | JWT `aud` claim; must match token audience. |
| **Auth** | SigningKey | string | (min 32 chars) | Symmetric key for JWT signing/validation. Use a long secret in production. |
| **Auth** | RequireHttps | bool | `false` | When true, JWT Bearer requires HTTPS metadata. |
| **GraphQL** | MaxDepth | int | `12` | Max allowed query depth; over it returns QUERY_TOO_DEEP. |
| **GraphQL** | MaxComplexity | int | `2000` | Max allowed complexity; over it returns QUERY_TOO_COMPLEX. |
| **GraphQL** | PersistedQueriesEnabled | bool | `false` | When true, HTTP POST /graphql only accepts requests with `extensions.persistedQuery.sha256Hash`; raw `query` is rejected (400 PERSISTED_ONLY). |
| **GraphQL** | PersistedQueriesPath | string | `"persisted-queries.json"` | Path to JSON file mapping sha256 hash → query text. Used by `FilePersistedQueryStore`. |
| **Redis** | ConnectionString | string | `""` | (verify in code) For future caching/pub-sub. |
| **Redis** | Enabled | bool | `false` | (verify in code) |

---

## Example appsettings.json (minimal)

```json
{
  "ConnectionStrings": {
    "ClaimsDb": "Server=localhost;Database=PayorClaimsGraphQL;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
  },
  "Seed": { "Enabled": true, "Reset": false },
  "Storage": { "Provider": "Local", "LocalPath": "App_Data/uploads" },
  "Auth": {
    "Issuer": "payorclaims",
    "Audience": "payorclaims",
    "SigningKey": "YOUR_32_OR_MORE_CHAR_SECRET_KEY_HERE",
    "RequireHttps": false
  },
  "GraphQL": {
    "MaxDepth": 12,
    "MaxComplexity": 2000,
    "PersistedQueriesEnabled": false,
    "PersistedQueriesPath": "persisted-queries.json"
  }
}
```

---

## Environment-specific behavior

- **Development:** Seed on/off via `Seed:Enabled`. Persisted queries typically off so Altair can send ad-hoc queries. Altair and Swagger (under `/admin/swagger`) are available when `IsDevelopment()`.
- **Testing:** `AddInfrastructure` uses `IHostEnvironment`; when `EnvironmentName == "Testing"` it registers SQLite with a shared in-memory DB (keeper connection + per-scope connections). Seed is disabled in test fixture config; tests call `EnsureDatabaseSeeded()` and use `JwtTestTokenFactory`. JWT option `MapInboundClaims = false` in Testing so short claim names (`role`, `sub`, `npi`, `memberId`) work. See [Testing strategy](../testing/testing-strategy.md).

---

## Commands (copy-paste)

```bash
# Add a new migration (after model changes)
dotnet ef migrations add YourMigrationName --project PayorClaims.Infrastructure --startup-project PayorClaims.Api

# Apply migrations to the database
dotnet ef database update --project PayorClaims.Infrastructure --startup-project PayorClaims.Api

# Run tests
dotnet test
```
