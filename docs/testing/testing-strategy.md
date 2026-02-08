# Testing strategy

Test setup (SQLite shared in-memory), how to run tests, what is covered, and how to add new tests.

---

## Why SQLite shared in-memory

Integration tests run against the full API (WebApplicationFactory) but **without SQL Server**. Using SQL Server in CI would require a real instance or container; SQLite in-memory keeps tests fast and deterministic with no external DB. A **shared** in-memory database (connection string with `Mode=Memory;Cache=Shared`) allows multiple connections to the same DB: the seed scope and each HTTP request use separate connections but see the same data.

---

## How it’s configured

- **Environment:** Test host uses `UseEnvironment("Testing")`. In `PayorClaims.Infrastructure.ServiceCollectionExtensions.AddInfrastructure`, when `env?.EnvironmentName == "Testing"`, the app registers:
  - A **keeper** `SqliteConnection` opened to `Data Source=payorclaims_test;Mode=Memory;Cache=Shared` (singleton), so the in-memory DB stays alive.
  - `AddDbContext<ClaimsDbContext>(opt => opt.UseSqlite(connectionString))` with the **same connection string**, so each scope gets its own connection to the same shared DB.
- **Migrations:** Tests use `EnsureCreated()` (not Migrate) so the schema is created from the current model. RowVersion for SQLite is configured with `ValueGeneratedNever()` so seed can set it. See `ClaimsDbContext.OnModelCreating` and test seed in `WebAppFixture`.
- **Seed:** Seed is disabled in test config. Tests call `WebAppFixture.EnsureDatabaseSeeded()` which creates the schema (if needed), inserts minimal data (one Member with SsnPlain, one Provider, one MemberConsent with ProviderAccessPHI = false), and sets `SeededMemberId` and `SeededProviderNpi` for assertions.

---

## What’s covered

- **Health / ping:** Unauthenticated ping and basic availability.
- **Auth and masking:** Adjuster sees full SSN; Provider without consent sees masked SSN; Member sees own SSN; unauthenticated SSN request gets masked response; raw SSN not leaked in response when unauthorized.
- **memberById resolution:** Seeded member is found by ID (validates IdGraphType string→Guid parsing and shared DB visibility).
- **Deterministic data:** Same seed every run; assertions on concrete values (e.g. SSN `123-45-6789`, masked form `***-**-****`).

(Verify in code for any tests that cover persisted-query endpoint behavior or other features.)

---

## How to run tests

```bash
# All tests
dotnet test

# Filter by test name
dotnet test --filter "Seeded_member_is_found_by_id"
```

Tests use the same API project (PayorClaims.Api) with Testing environment and in-memory config (Auth:SigningKey, Auth:Issuer, Auth:Audience; Seed:Enabled false). JWT tokens are built by `JwtTestTokenFactory` with the same config so the custom GraphQL endpoint (when it builds actor from Bearer token) sees the correct roles and memberId.

---

## How to add new tests

1. Use **IClassFixture&lt;WebAppFixture&gt;** so the same host (and shared DB) is reused across tests in the class.
2. Call **EnsureDatabaseSeeded()** if the test needs the seeded member/provider/consent.
3. Build a client with **CreateClientWithToken(role, sub, npi, memberId)** when you need an authenticated user (e.g. Adjuster, Provider, Member with memberId).
4. POST to `/graphql` with `PostAsJsonAsync("/graphql", new { query = "..." })`. For mutations, include `query` and optionally `variables`.
5. Assert on HTTP status, then on `response.Content` (e.g. parse JSON and check `data` and `errors`). Use FluentAssertions or similar for clarity.

Avoid depending on test order; each test should be able to run in isolation with a fresh seed (or shared seed) as appropriate.
