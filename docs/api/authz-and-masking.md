# Authorization and masking

JWT roles, authorization policies, consent rules, and field-level masking (SSN, email, phone) for the GraphQL API.

---

## Roles

| Role | Description |
|------|-------------|
| **Admin** | Full access; can register/deactivate webhooks, manage exports. |
| **Adjuster** | Can read full member PHI (including SSN when `SsnPlain` is set), adjudicate claims. |
| **Provider** | Can submit claims, view member data subject to consent; **never** sees full SSN. |
| **Member** | Can see own record (including own SSN when allowed); exports for self. |
| **System / Anonymous** | No roles; sensitive fields are masked. |

Roles are derived from JWT claims (see [JWT claims](#jwt-claims)) and stored in `ActorContext` by `ActorContextMiddleware` (or by the custom GraphQL endpoint when building actor from the Bearer token).

---

## Authorization policies

Defined in `PayorClaims.Api` (AddAuthorization). Used by REST controllers and can be applied to GraphQL if needed (verify in code for exact usage):

| Policy | Required roles |
|--------|-----------------|
| **CanReadMember** | Admin, Adjuster, Provider, Member |
| **CanAdjudicate** | Admin, Adjuster |
| **CanSubmitClaim** | Admin, Adjuster, Provider |
| **AdminOnly** | Admin |

GraphQL resolvers typically use `IActorContextProvider.GetActorContext()` and check `actor.IsAdmin`, `actor.IsAdjuster`, `actor.IsProvider`, `actor.IsMember` and `actor.MemberId` for field-level behavior (e.g. SSN masking).

---

## Sensitive fields

Fields that are masked or restricted based on role and consent:

| Field | Entity / type | Behavior |
|-------|----------------|--------|
| **ssn** | Member | See [SSN rules](#ssn-rules). |
| **email** | Member | Masked unless consent/role allows (verify in code: `MemberType` email resolver). |
| **phone** | Member | Masked unless consent/role allows (verify in code: `MemberType` phone resolver). |
| **dob** | Member | Can be masked via `Masking.MaskDob()` when not allowed (verify in code). |

Implementations live in `PayorClaims.Schema/Types/MemberType.cs` and `PayorClaims.Application.Security.Masking`.

---

## SSN rules

- **Admin / Adjuster:** Full SSN when `SsnPlain` is set; otherwise masked (`***-**-****`).
- **Member:** Full SSN only for **self** (actor.MemberId == member.Id) when `SsnPlain` is set; otherwise masked.
- **Provider:** **Never** full SSN; always masked regardless of consent.
- **Anonymous / no role:** Masked.

Source: `MemberType` SSN resolver in `PayorClaims.Schema/Types/MemberType.cs`.

---

## Consent rules

Provider access to PHI (e.g. email, phone for contact) can depend on consent. Consent is checked via `IConsentService.HasConsentAsync(memberId, consentType)`.

- **Consent type for provider PHI access:** e.g. **ProviderAccessPHI** (verify in code: `MemberType` and `ConsentService`; consent types may include `EmailContact`, `PhoneContact`).
- **Logic:** If actor is Provider, resolvers call `HasConsentAsync(memberId, "EmailContact")` or similar before returning unmasked data. If no consent, return masked value.
- **Extending:** Add new consent types in `MemberConsent.ConsentType` and new checks in the relevant type resolvers. See `PayorClaims.Infrastructure.Services.ConsentService` and `PayorClaims.Schema.Types.MemberType`.

---

## JWT claims

The API expects these claims (short names when `MapInboundClaims` is false, e.g. in Testing):

| Claim | Description |
|-------|-------------|
| **sub** | Subject (user id); stored as `ActorContext.ActorId` (Guid). |
| **role** | Single role string (e.g. `"Adjuster"`). Stored in `ActorContext.Roles`. |
| **npi** | Provider NPI when actor is a provider. |
| **memberId** | Member id when actor is a member (so “self” can be determined). |

Middleware: `ActorContextMiddleware` reads `context.User` after JWT Bearer authentication and fills `ActorContext` in `HttpContext.Items`. The custom GraphQL POST handler also builds the actor from the Bearer token when the user is not authenticated (e.g. in test host) so resolvers still see the correct role and memberId.

---

## Testing note

Integration tests use **SQLite** and **JWT tokens** built by `JwtTestTokenFactory` with the same signing key as the test host. In **Testing** environment the API sets **MapInboundClaims = false** so claim names stay as `role`, `sub`, `npi`, `memberId`. Tests pass these claims to simulate Adjuster, Provider, and Member; masking tests assert that Provider sees masked SSN and Adjuster/Member (self) see full SSN when `SsnPlain` is set. See [Testing strategy](../testing/testing-strategy.md) and [Common issues – JWT](../troubleshooting/common-issues.md).
