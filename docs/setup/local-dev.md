# Local development

Step-by-step guide to run the PayorClaims API locally, with ports and troubleshooting.

---

## Prerequisites

- **.NET SDK** (sdk 10 or the version in `global.json` if present). Check: `dotnet --version`.
- **SQL Server** (LocalDB or full instance) for normal development. For tests only, SQL Server is not required (SQLite in-memory is used).

---

## Step-by-step run

### 1. Clone and build

```bash
cd PayorClaimsGraphQL
dotnet build
```

### 2. Configure database (optional)

Default `appsettings.json` uses:

```json
"ConnectionStrings": {
  "ClaimsDb": "Server=localhost;Database=PayorClaimsGraphQL;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
}
```

Adjust `Server` if needed (e.g. `(localdb)\\mssqllocaldb`). See [Configuration](configuration.md).

### 3. Apply migrations (if not using seed)

If you prefer to apply migrations yourself and skip seed:

```bash
dotnet ef database update --project PayorClaims.Infrastructure --startup-project PayorClaims.Api
```

If **Seed:Enabled** is `true`, the app runs migrations and seed on startup (see [Configuration](configuration.md)).

### 4. Run the API

```bash
dotnet run --project PayorClaims.Api
```

Default launch profile is **http**: app listens on **http://localhost:5194**.

To use HTTPS as well:

```bash
dotnet run --project PayorClaims.Api --launch-profile https
```

Then the app uses **https://localhost:7135** and **http://localhost:5194** (see [Ports](#ports)).

### 5. Verify

- **Health:** `curl http://localhost:5194/health/live` or `http://localhost:5194/health/ready`
- **GraphQL:** Open http://localhost:5194/ui/altair and run `query { ping }`
- **Subscriptions:** In Altair, switch to WebSocket URL `ws://localhost:5194/graphql` and subscribe (e.g. `claimStatusChanged(claimId: "...")`)

---

## Ports

Defined in `PayorClaims.Api/Properties/launchSettings.json`:

| Profile | ApplicationUrl | Use |
|--------|----------------|-----|
| **http** | `http://localhost:5194` | Default; HTTP only. |
| **https** | `https://localhost:7135;http://localhost:5194` | HTTPS on 7135, HTTP on 5194. |

So:

- **HTTP:** 5194  
- **HTTPS:** 7135  
- **GraphQL:** http://localhost:5194/graphql (or https://localhost:7135/graphql with https profile)  
- **Altair:** http://localhost:5194/ui/altair  
- **WebSocket:** ws://localhost:5194/graphql (or wss://localhost:7135/graphql with https)

---

## Environment behavior

| Environment | Typical use | Seed | Persisted queries | DB |
|-------------|-------------|------|--------------------|-----|
| **Development** | Local run | On or off via config | Usually off (so Altair can send ad‑hoc queries) | SQL Server |
| **Testing** | `dotnet test` | Disabled in test config | Off | SQLite shared in-memory |

`ASPNETCORE_ENVIRONMENT` is set by the launch profile (e.g. Development). Tests set `UseEnvironment("Testing")` so Infrastructure uses SQLite. See [Configuration](configuration.md) and [Testing strategy](../testing/testing-strategy.md).

---

## Troubleshooting

- **Port in use:** Change the port in `launchSettings.json` under the profile’s `applicationUrl`, or stop the process using 5194/7135.
- **DB connection failed:** Check `ConnectionStrings:ClaimsDb`, SQL Server running, and firewall. For LocalDB: `Server=(localdb)\\mssqllocaldb;...`.
- **Migrations pending:** Run `dotnet ef database update --project PayorClaims.Infrastructure --startup-project PayorClaims.Api`. If you see “pending model changes”, add a migration first: `dotnet ef migrations add YourName --project PayorClaims.Infrastructure --startup-project PayorClaims.Api`.
- **Altair returns 400 for queries:** If **GraphQL:PersistedQueriesEnabled** is `true`, POST /graphql only accepts persisted queries (hash). Set to `false` for local dev or add your query to `persisted-queries.json`. See [Troubleshooting – persisted queries](../troubleshooting/common-issues.md).
- **Subscriptions don’t connect:** Ensure `UseWebSockets()` is before `MapGraphQL` in `Program.cs`, and use `ws://` (not `wss://`) when using the http profile. See [Common issues](../troubleshooting/common-issues.md).
