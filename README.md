# PayorClaims GraphQL

A **healthcare payor claims adjudication demo** built with **GraphQL.NET** and **EF Core**. It models a simplified payor system: members, plans, coverages, providers, claims, adjudication, EOBs, and exports, with JWT-based auth, field-level masking, consent rules, and HIPAA-oriented audit trails.

## What this system does

- **GraphQL API** for querying members, claims, providers, coverages; submitting and adjudicating claims; uploading attachments; requesting exports; and subscribing to claim/EOB events.
- **SQL Server** (or SQLite in-memory for tests) via EF Core, with soft delete, `RowVersion` concurrency, idempotency keys, and duplicate fingerprints.
- **HIPAA-style access logs** (append-only, hash-chained) and **audit events** (append-only, hash-chained, hourly validation job).
- **JWT auth** with roles (Admin, Adjuster, Provider, Member); **field masking** (SSN, email, phone) and **consent** (e.g. ProviderAccessPHI).
- **Subscriptions** over WebSockets (in-memory event bus) for `claimStatusChanged` and `eobGenerated`.
- **Webhooks**: DB-backed queue, retries with backoff, HMAC signing with timestamp for replay protection.
- **Export jobs** with one-time secure download tokens and a dedicated download endpoint.
- **Query limits**: configurable max depth and complexity.
- **Persisted queries**: optional HTTP enforcement (when enabled, only stored queries by hash are accepted).

---

## Quickstart

```bash
# Build
dotnet build

# Run API (default profile: HTTP on port 5194)
dotnet run --project PayorClaims.Api
```

Then open:

| What | URL |
|------|-----|
| **GraphQL HTTP** | http://localhost:5194/graphql |
| **Altair IDE** | http://localhost:5194/ui/altair |
| **Subscriptions (WS)** | ws://localhost:5194/graphql |

With HTTPS profile: `https://localhost:7135` and `http://localhost:5194` (see [Local development](docs/setup/local-dev.md)).

---

## Documentation

All technical documentation lives under **[/docs](docs/README.md)**.

| Section | Description |
|--------|-------------|
| [Documentation index](docs/README.md) | Full list of docs with links |
| [Architecture](docs/architecture/overview.md) | Layers, request flows, key decisions |
| [Project structure](docs/architecture/project-structure.md) | Solution layout and responsibilities |
| [Local development](docs/setup/local-dev.md) | Step-by-step run, ports, troubleshooting |
| [Configuration](docs/setup/configuration.md) | Config keys (Auth, GraphQL, Seed, Storage, DB) |
| [GraphQL API](docs/api/graphql.md) | Endpoints, schema, sample operations |
| [Auth & masking](docs/api/authz-and-masking.md) | JWT roles, consent, masking rules |
| [Database](docs/database/README.md) | Design, ERD, invariants, migrations |
| [HIPAA & audit](docs/security/hipaa-audit.md) | Access logs, audit chain, validation |
| [Webhooks](docs/ops/webhooks.md) | Endpoints, signing, retries |
| [Exports](docs/ops/exports.md) | Job lifecycle, download tokens |
| [N+1 & DataLoader](docs/performance/n-plus-one.md) | Batching strategy |
| [Testing](docs/testing/testing-strategy.md) | SQLite in-memory, how to run tests |
| [Troubleshooting](docs/troubleshooting/common-issues.md) | Common failures and fixes |

---

## Solution layout

- **PayorClaims.Api** – ASP.NET Core host, middleware, custom GraphQL POST endpoint, controllers.
- **PayorClaims.Schema** – GraphQL schema (queries, mutations, subscriptions), types, DataLoaders.
- **PayorClaims.Application** – DTOs, validation, event contracts, security interfaces (e.g. `IActorContextProvider`, `IConsentService`).
- **PayorClaims.Infrastructure** – EF Core, migrations, services, webhooks, export worker, audit job, persisted query store.
- **PayorClaims.Domain** – Entities and base types.
- **PayorClaims.Tests** – Integration tests (SQLite in-memory, JWT helpers).

See [Project structure](docs/architecture/project-structure.md) for details.
