# PayorClaims GraphQL — Documentation index

Technical documentation for the PayorClaims GraphQL + EF Core payor claims system. Use this index to jump to a topic.

---

## Architecture

| Doc | Description |
|-----|-------------|
| [Architecture overview](architecture/overview.md) | Layers, dependency rules, request flows (GraphQL, mutations, subscriptions, webhooks, exports) |
| [Project structure](architecture/project-structure.md) | Solution layout, project roles, where key decisions live |

---

## Setup & configuration

| Doc | Description |
|-----|-------------|
| [Local development](setup/local-dev.md) | Step-by-step local run, ports (5194/7135), troubleshooting |
| [Configuration](setup/configuration.md) | All config keys with examples (Auth, GraphQL, Seed, Storage, DB) |

---

## API

| Doc | Description |
|-----|-------------|
| [GraphQL API](api/graphql.md) | Endpoints, Altair, schema discovery, sample queries/mutations/subscriptions, errors |
| [Auth & masking](api/authz-and-masking.md) | JWT roles, policies, consent rules, masking behavior, examples |

---

## Database

| Doc | Description |
|-----|-------------|
| [Database README](database/README.md) | Design explanation, invariants, migration workflow, ERD |
| [ERD (Mermaid)](database/erd.mmd) | Mermaid ER diagram source (core tables and relations) |
| [Tables reference](database/tables.md) | Table-by-table reference (columns, purpose, relations, constraints) |

---

## Security & compliance

| Doc | Description |
|-----|-------------|
| [HIPAA & audit](security/hipaa-audit.md) | HIPAA access logs, hash chain, audit events, immutability, validation job |

---

## Operations

| Doc | Description |
|-----|-------------|
| [Webhooks](ops/webhooks.md) | Webhook endpoints, signing (HMAC + timestamp), retries, delivery statuses |
| [Exports](ops/exports.md) | Export job lifecycle, secure download tokens, endpoint usage; query limits and persisted queries |

---

## Performance & testing

| Doc | Description |
|-----|-------------|
| [N+1 and DataLoader](performance/n-plus-one.md) | DataLoader strategy, batching rules, how to verify N+1 is avoided |
| [Testing strategy](testing/testing-strategy.md) | Test setup (SQLite shared in-memory), how to run tests, coverage |

---

## Troubleshooting

| Doc | Description |
|-----|-------------|
| [Common issues](troubleshooting/common-issues.md) | Seed, persisted queries, IdGraphType GUID parsing, WebSockets, JWT claims |
