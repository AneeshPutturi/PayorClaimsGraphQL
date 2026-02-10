# PayorClaims Angular Frontend

A production-grade Angular UI for the PayorClaims GraphQL backend.

## Tech Stack

- **Angular 19+** with NgModules + lazy-loaded feature routes
- **Angular Material** — toolbar, sidenav, tables, stepper, dialogs, snackbars
- **Apollo Angular** — GraphQL client with HTTP + WebSocket (subscriptions) split link
- **GraphQL Code Generator** — typed operations from schema introspection
- **JWT authentication** — in-memory token store, role-based guards and UI

## Prerequisites

- Node.js 20+
- Backend running at `http://localhost:5194` (see root project README)

## Quick Start

```bash
cd frontend

# Install dependencies
npm install

# Start dev server
ng serve
# or
npm start
```

Open `http://localhost:4200`. You'll see the login page — paste a JWT token from the backend.

## Backend URLs

Configured in `src/environments/environment.ts`:

| Setting | Default |
|---------|---------|
| `apiUrl` | `http://localhost:5194/graphql` |
| `wsUrl` | `ws://localhost:5194/graphql` |
| `exportsBaseUrl` | `http://localhost:5194` |
| `altairUrl` | `http://localhost:5194/ui/altair` |

For production, update `environment.prod.ts`.

## GraphQL Code Generation

If the backend is running and introspection is enabled:

```bash
npx graphql-codegen
```

This generates typed TypeScript services in `src/app/graphql/generated.ts` from the `.graphql` operation files in `src/app/graphql/operations/`.

## Project Structure

```
src/app/
├── core/
│   ├── auth/          # AuthService, guards, models
│   ├── graphql/       # Apollo config (HTTP + WS split link, error handling)
│   ├── layout/        # Shell, login, forbidden components
│   └── notifications/ # Subscription-based notification service + drawer
├── shared/
│   ├── components/    # StatusBadge, Loading, EmptyState
│   └── pipes/         # MoneyPipe
├── features/
│   ├── member360/     # Member search + Member 360 detail page
│   ├── claims/        # Claim list, detail (with attachments), submission wizard
│   ├── adjudication/  # Work queue + adjudication editor (concurrency-safe)
│   ├── exports/       # Export request + polling + one-time download token
│   ├── admin-webhooks/# Register/deactivate webhook endpoints
│   └── docs/          # GraphQL explorer link + sample queries + troubleshooting
├── graphql/
│   └── operations/    # .graphql files (member, claims, providers, exports, etc.)
└── environments/      # API URLs per environment
```

## Feature Slices

| Slice | Route | Roles | Description |
|-------|-------|-------|-------------|
| Member 360 | `/members`, `/members/:id` | All | Search + detailed member view with coverage, benefits, claims |
| Claims | `/claims`, `/claims/:id`, `/claims/submit` | All / Provider | List, detail (with attachments + appeals), multi-step submission wizard |
| Adjudication | `/adjudication`, `/adjudication/:id` | Admin, Adjuster | Work queue + line-level adjudication with concurrency control |
| Exports | `/exports` | Member, Admin | Request export, poll status, one-time download token |
| Webhooks | `/admin/webhooks` | Admin | Register and deactivate webhook endpoints |
| Docs | `/docs` | All | Sample queries, Altair link, troubleshooting guide |

## Authentication

1. Get a JWT from the backend (e.g. via the test token factory or your auth endpoint)
2. Paste it in the login page
3. The token is stored **in memory only** (not localStorage) — refresh = logout
4. Apollo attaches `Authorization: Bearer <token>` to every request
5. Route guards enforce role-based access

## Common Errors

| Error | Cause | Fix |
|-------|-------|-----|
| 401 on queries | Token expired or missing | Re-login with a fresh JWT |
| CONCURRENCY_CONFLICT | Another user modified the claim | Click "Reload" and retry |
| PERSISTED_ONLY | Backend has persisted queries enabled | Set `PersistedQueriesEnabled=false` in backend dev config |
| QUERY_TOO_DEEP | Query nesting exceeds MaxDepth | Simplify the query |
