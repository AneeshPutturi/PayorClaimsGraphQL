# Architecture overview

This document describes the high-level architecture: project layers, dependency rules, and main request flows.

---

## Projects and layers

| Project | Role |
|--------|------|
| **PayorClaims.Api** | ASP.NET Core host. Middleware (auth, ActorContext, correlation). Custom GraphQL HTTP POST endpoint (persisted-query enforcement). Controllers (e.g. export download). No business logic. |
| **PayorClaims.Schema** | GraphQL schema: `AppSchema`, `AppQuery`, `AppMutation`, `AppSubscription`. Types, inputs, DataLoaders. Resolvers call Application/Infrastructure services via `RequestServices`. Does **not** reference ASP.NET Core; uses `IActorContextProvider` from Application. |
| **PayorClaims.Application** | Abstractions: `IClaimService`, `IExportService`, `IWebhookService`, `IPersistedQueryStore`, `IActorContextProvider`, `IConsentService`, `IAccessLogger`. DTOs, FluentValidation validators, event contracts (`IEventBus`, `ClaimStatusChangedEvent`, `EobGeneratedEvent`), `Masking` and security types. |
| **PayorClaims.Infrastructure** | Implementations: EF Core `ClaimsDbContext`, migrations, configurations. `ClaimService`, `ExportService`, `WebhookService`, `ConsentService`, `AccessLogger`, `FilePersistedQueryStore`. Background services: `WebhookDeliveryWorker`, `ExportJobWorker`, `WebhookEventSubscriber`, `AuditChainValidationJob`. In-memory event bus. |
| **PayorClaims.Domain** | Entities (`Member`, `Claim`, `Plan`, etc.) and `BaseEntity`. No dependencies on other projects. |
| **PayorClaims.Tests** | Integration tests; references Api + all other projects. Uses `WebApplicationFactory`, SQLite in-memory, `JwtTestTokenFactory`. |

**Dependency direction:** Api → Schema, Application, Infrastructure; Schema → Application (and Infrastructure for persistence types); Application ← Infrastructure; Domain is referenced by everyone. Schema must not depend on Api (so it stays transport-agnostic).

---

## Where key decisions live

| Decision | Location |
|----------|----------|
| DataLoaders (batching) | `PayorClaims.Schema/Loaders/` (e.g. `CoveragesByMemberIdLoader`, `PlanByIdLoader`) |
| Business rules (submit/adjudicate claim, export) | `PayorClaims.Application` (interfaces) + `PayorClaims.Infrastructure/Services/` |
| EF configs and migrations | `PayorClaims.Infrastructure/Persistence/`, `Migrations/` |
| Append-only enforcement (HipaaAccessLog, AuditEvent) | `ClaimsDbContext.SaveChanges` / `SaveChangesAsync` |
| Persisted query store | `PayorClaims.Infrastructure/PersistedQueries/FilePersistedQueryStore.cs`; path from `GraphQL:PersistedQueriesPath` |

---

## Request flows

### 1) GraphQL query (HTTP)

```
HTTP POST /graphql (or GET via MapGraphQL)
  → Auth middleware (JWT) → ActorContext middleware (build actor from User)
  → Custom POST handler (if POST): deserialize body, persisted-query check, build ExecutionOptions
  → IDocumentExecuter.ExecuteAsync(Schema, Query, RequestServices, UserContext with ActorContext)
  → Resolvers: RequestServices.GetRequiredService<ClaimsDbContext> / DataLoader / IActorContextProvider
  → Response JSON
```

**Why this matters:** Resolvers get scoped `ClaimsDbContext` and `IActorContextProvider` from the request. For ID arguments, resolvers must parse string IDs to `Guid` (IdGraphType exposes strings). See [Troubleshooting](troubleshooting/common-issues.md).

### 2) Mutation (validation → service → transaction → events)

```
Mutation (e.g. submitClaim, adjudicateClaim)
  → FluentValidation (Application validators)
  → IClaimService (Infrastructure) inside DbContext transaction
  → SaveChanges (concurrency, append-only checks, audit hash chain)
  → IEventBus.Publish (ClaimStatusChangedEvent / EobGeneratedEvent)
  → Response (claim / payload)
```

Subscribers (WebhookEventSubscriber, subscription resolvers) react to events after the transaction.

### 3) Subscription (WebSocket)

```
WebSocket ws://host/graphql
  → MapGraphQL handles WS; subscription resolver (e.g. claimStatusChanged(claimId))
  → IEventBus.Subscribe<ClaimStatusChangedEvent>().Where(claimId)
  → On event: load Claim from DbContext in new scope → yield to client
```

Subscription resolvers use `ResolveStream` and `Observable.FromAsync` with a new scope per event so each resolution gets a fresh DbContext.

### 4) Webhook delivery

```
Event published → WebhookEventSubscriber enqueues WebhookDelivery (Pending, NextAttemptAt = now)
  → WebhookDeliveryWorker (every 5s): load Pending deliveries where NextAttemptAt <= now
  → POST to endpoint with X-Webhook-Timestamp, X-Webhook-Signature (HMAC over "{timestamp}.{payload}")
  → On success: Status = Succeeded. On failure: backoff, increment AttemptCount; after MaxAttempts → Failed
```

See [Webhooks](ops/webhooks.md).

### 5) Export flow

```
Mutation requestMemberClaimsExport(memberId) → ExportService creates ExportJob (Queued)
  → ExportJobWorker (every 10s): pick Queued job → Running → write JSON to Storage → Ready, set DownloadTokenHash, ExpiresAt
  → Client uses downloadTokenOnce with GET /exports/{jobId}/download?token=...
  → ExportDownloadController: validate token hash, expiry, status; return file or 4xx
```

See [Exports](ops/exports.md).
