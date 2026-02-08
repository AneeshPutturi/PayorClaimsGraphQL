# Project structure

This page explains the solution layout and why projects are organized this way.

---

## Solution layout

```
PayorClaimsGraphQL/
├── PayorClaims.Api/           # Host, HTTP, middleware, controllers
├── PayorClaims.Schema/        # GraphQL schema, types, DataLoaders
├── PayorClaims.Application/   # Interfaces, DTOs, validation, events
├── PayorClaims.Infrastructure/# EF Core, services, workers, persistence
├── PayorClaims.Domain/        # Entities only
├── PayorClaims.Tests/         # Integration tests
└── docs/                      # Documentation
```

---

## PayorClaims.Api

- **Purpose:** Run the app; wire HTTP, auth, and GraphQL.
- **Key pieces:**
  - `Program.cs` – builder, middleware order, `MapPost("/graphql", ...)` (custom), `MapGraphQL("/graphql")` (GET/WS), Altair, health, seed.
  - `GraphQL/GraphQLHttpEndpoint.cs` – custom POST handler: body → persisted-query rules → `ExecutionOptions` (RequestServices, UserContext with ActorContext) → execute → JSON. Optional actor-from-Bearer when auth middleware did not run (e.g. tests).
  - `Middleware/ActorContextMiddleware.cs` – build `ActorContext` from JWT claims; store in `HttpContext.Items`.
  - `Controllers/ExportDownloadController.cs` – `GET /exports/{jobId}/download?token=...`.
  - `Options/` – `AuthOptions`, `GraphQLOptions`, `StorageOptions`, etc.
- **Why:** Keeps HTTP and hosting in one place; Schema stays unaware of ASP.NET Core.

---

## PayorClaims.Schema

- **Purpose:** Define and execute GraphQL: queries, mutations, subscriptions, types, and batching.
- **Key pieces:**
  - `Schema/AppSchema.cs`, `AppQuery.cs`, `AppMutation.cs`, `AppSubscription.cs`.
  - `Types/` – `MemberType`, `ClaimType`, `PlanType`, `CoverageType`, `ProviderType`, etc.
  - `Inputs/` – `ClaimSubmissionInputGraphType`, `AdjudicateClaimInput`, etc.
  - `Loaders/` – DataLoaders (e.g. `CoveragesByMemberIdLoader`, `PlanByIdLoader`, `ClaimLinesByClaimIdLoader`).
- **Dependencies:** Application (for `IActorContextProvider`, `IConsentService`), Infrastructure (for `ClaimsDbContext`, entity types). No reference to Api.
- **Why:** Schema is the single place for GraphQL shape and resolver wiring; DataLoaders here keep N+1 under control.

---

## PayorClaims.Application

- **Purpose:** Contracts and shared concepts used by Schema and Infrastructure.
- **Key pieces:**
  - `Abstractions/` – `IClaimService`, `IExportService`, `IWebhookService`, `IPersistedQueryStore`, `ISeedRunner`, `IAccessLogger`, `IClaimAttachmentService`.
  - `Security/` – `ActorContext`, `IActorContextProvider`, `IConsentService`, `Masking`.
  - `Events/` – `IEventBus`, `ClaimStatusChangedEvent`, `EobGeneratedEvent`.
  - `Dtos/Claims/` – `ClaimSubmissionInputDto`, `AdjudicateLineDto`, etc.
  - `Validation/` – FluentValidation validators for submission/adjudication inputs.
  - `Exceptions/` – `AppValidationException` (code + errors list).
- **Why:** Schema and Infrastructure depend on Application, not on each other’s concrete types; test and swap implementations without touching Schema.

---

## PayorClaims.Infrastructure

- **Purpose:** Persistence, external I/O, background jobs.
- **Key pieces:**
  - `Persistence/` – `ClaimsDbContext`, `Configurations/`, `Migrations/`.
  - `Services/` – `ClaimService`, `ExportService`, `WebhookService`, `ConsentService`, `AccessLogger`, `ClaimAttachmentService`.
  - `PersistedQueries/FilePersistedQueryStore.cs` – load JSON map (hash → query); used by custom GraphQL endpoint.
  - `Webhooks/WebhookDeliveryWorker.cs`, `WebhookEventSubscriber.cs`.
  - `Export/ExportJobWorker.cs`.
  - `Audit/AuditChainValidationJob.cs`.
  - `Events/InMemoryEventBus.cs`.
  - `Seed/SeedRunner.cs`.
  - `ServiceCollectionExtensions.cs` – register DbContext (SQL Server or SQLite for Testing), services, workers.
- **Why:** All EF and “out of process” behavior lives here; Api stays thin.

---

## PayorClaims.Domain

- **Purpose:** Entities and base types only.
- **Key pieces:** `Entities/` (Member, Claim, Plan, Coverage, Provider, etc.), `Common/BaseEntity`.
- **Why:** No dependencies on other projects; shared by Application (DTOs may mirror entities) and Infrastructure (mappings and DbSets).

---

## PayorClaims.Tests

- **Purpose:** Integration tests against the running API with in-memory DB.
- **Key pieces:** `WebAppFixture` (WebApplicationFactory, Testing env, SQLite), `JwtTestTokenFactory`, `Integration/AuthAndMaskingTests.cs`.
- **Why:** Tests run without SQL Server; shared in-memory DB and JWT tokens make auth/masking and member-by-id behavior deterministic. See [Testing strategy](testing/testing-strategy.md).
