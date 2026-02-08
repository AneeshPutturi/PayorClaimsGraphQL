# Common issues and fixes

Frequent failures and how to resolve them. No application code changes; docs and config only.

---

## Altair returns 400 for queries

**Symptom:** POST to `/graphql` with a raw `query` returns 400.

**Cause:** **GraphQL:PersistedQueriesEnabled** is `true`. The custom POST handler rejects requests that send a `query` string and requires `extensions.persistedQuery.sha256Hash` instead.

**Fix:** For local development, set `PersistedQueriesEnabled: false` in appsettings (or appsettings.Development.json). For production with persisted queries, use a client that sends the hash and add the query to `persisted-queries.json`. See [Exports – Persisted queries](../ops/exports.md#persisted-queries).

---

## Seed runs but tables appear empty

**Symptom:** Seed is enabled and runs at startup, but queries return no rows.

**Checks:**

1. **Seed:Enabled** – Must be `true` for seed to run. Check config (appsettings, environment).
2. **Migrations** – Database must be created and migrated. Run `dotnet ef database update --project PayorClaims.Infrastructure --startup-project PayorClaims.Api`. If there are pending model changes, add a migration first.
3. **Connection string** – Verify `ConnectionStrings:ClaimsDb` points to the instance you are querying (e.g. not a different database or server).
4. **Logs** – Check startup logs for seed or migration errors. In Development, Serilog and EF logging can show SQL and exceptions.

---

## memberById (or claimById / eobById) returns null

**Symptom:** Query returns `data.memberById: null` even though the entity exists in the DB.

**Cause:** GraphQL.NET’s **IdGraphType** supplies ID arguments as **strings** in resolvers. If the resolver uses `GetArgument<Guid>("id")` without parsing, it can get `default(Guid)` and the lookup returns null.

**Fix:** In the resolver, get the id as string and parse: `var idStr = c.GetArgument<string>("id"); if (string.IsNullOrEmpty(idStr) || !Guid.TryParse(idStr, out var id)) return null;` then use `id` in the query. This is already done in `AppQuery` for `memberById`, `claimById`, and `eobById`. If you add new by-id fields with IdGraphType, use the same pattern. See [GraphQL API – ID arguments](../api/graphql.md#important-id-arguments-guid-parsing).

---

## Subscriptions don’t connect

**Symptom:** WebSocket connection to `/graphql` fails or subscriptions never receive events.

**Checks:**

1. **UseWebSockets** – `app.UseWebSockets()` must be called before `MapGraphQL` in `Program.cs` (it is in the default template).
2. **URL** – Use `ws://localhost:5194/graphql` for HTTP or `wss://localhost:7135/graphql` for HTTPS. Do not use `http`/`https` in the WebSocket URL.
3. **Endpoint** – Subscriptions are served by `MapGraphQL<AppSchema>("/graphql")` (same path as HTTP). The custom POST handler only handles POST; GET and WebSocket are handled by MapGraphQL.
4. **Events** – Subscriptions react to `IEventBus` (in-memory). Events are published when mutations complete (e.g. adjudicateClaim publishes ClaimStatusChanged). If no event is published, the subscription will not fire.

---

## JWT roles not applied (masking wrong)

**Symptom:** Authenticated user with role “Adjuster” or “Member” still sees masked SSN, or Provider sees full SSN.

**Checks:**

1. **Issuer / Audience** – Token must be issued with the same `iss` and `aud` as configured in **Auth:Issuer** and **Auth:Audience**. Mismatch causes validation failure and user is treated as unauthenticated.
2. **Signing key** – **Auth:SigningKey** must match the key used to sign the token (same length and value). In tests, the fixture sets the same key in config and `JwtTestTokenFactory` uses it.
3. **Claims mapping** – In production the JWT handler may map claim names (e.g. `role` → `http://.../claims/role`). The middleware and custom endpoint read both `ClaimTypes.Role` and `"role"`. In **Testing**, the API sets **MapInboundClaims = false** so short names (`role`, `sub`, `npi`, `memberId`) are preserved. If roles still don’t apply, ensure the token includes the role claim and that the endpoint or middleware is building `ActorContext` from the same User or token. See [Auth & masking](../api/authz-and-masking.md) and [Testing strategy](../testing/testing-strategy.md).
