# Webhooks

Webhook endpoints, payload signing (HMAC + timestamp), retry policy, and delivery statuses.

---

## Registering and deactivating endpoints

- **registerWebhook(name, url, secret):** Creates a webhook endpoint. **Admin-only.** Returns the created endpoint (id, name, url, isActive, createdAt). Secret is optional but recommended for signature verification.
- **deactivateWebhook(id):** Sets the endpoint inactive. **Admin-only.** Further deliveries are not sent to deactivated endpoints.

Both are GraphQL mutations in `AppMutation`; they call `IWebhookService` in Infrastructure.

---

## Delivery worker

**Implementation:** `PayorClaims.Infrastructure.Webhooks.WebhookDeliveryWorker` (background service).

**Schedule:** Runs every **5 seconds** (interval in code: `WebhookDeliveryWorker.Interval`).

**Logic:**

1. Load deliveries where `Status = 'Pending'` and `NextAttemptAt <= UtcNow`, up to 50.
2. Resolve active endpoints by id.
3. For each delivery: POST to endpoint URL with headers and body; compute signature (see below).
4. Update delivery: `LastAttemptAt`, `LastStatusCode`, `LastError`, `AttemptCount++`. If success → `Status = 'Succeeded'`, `NextAttemptAt = null`. If failure and `AttemptCount < MaxAttempts` → set `NextAttemptAt` to now + backoff; else → `Status = 'Failed'`.

---

## Retry backoff and max attempts

- **MaxAttempts:** **8** (in code: `WebhookDeliveryWorker.MaxAttempts`).
- **Backoff (in order):** 1 min, 5 min, 15 min, 1 h, 6 h, 24 h. The index is `Min(AttemptCount - 1, Backoff.Length - 1)`, so later attempts use 24 h. (Verify in code: `WebhookDeliveryWorker.Backoff`.)

---

## Signature (replay protection)

**Purpose:** Receivers can verify that the request came from the app and was not replayed, by verifying the signature over a payload that includes a timestamp.

**Algorithm:**

- **Timestamp:** Unix seconds (UTC), sent in header **X-Webhook-Timestamp**.
- **Signed payload:** `"{timestamp}.{payloadJson}"` (timestamp string + dot + raw JSON body).
- **Signature:** HMAC-SHA256(secret, UTF-8 bytes of signed payload), output as **hex** (lowercase). Sent in header **X-Webhook-Signature** as `sha256=<hex>`.

**Headers sent:**

| Header | Description |
|--------|-------------|
| X-Webhook-EventType | Event type (e.g. ClaimStatusChanged, EobGenerated). |
| X-Webhook-DeliveryId | Delivery record id (GUID). |
| X-Webhook-Timestamp | Unix seconds (replay window can be enforced by receiver). |
| X-Webhook-Signature | `sha256=<hex>` of HMAC over `"{timestamp}.{payloadJson}"`. |

**Receiver verification (recommended):** Recompute HMAC with the shared secret over `"{timestamp}.{body}"` and compare to X-Webhook-Signature; optionally reject if timestamp is too old (e.g. > 5 minutes).

---

## Delivery statuses

| Status | Meaning |
|--------|---------|
| **Pending** | Queued; will be sent when `NextAttemptAt` is due. |
| **Succeeded** | HTTP response was successful (2xx). |
| **Failed** | Max attempts reached without success. |

(Verify in code for any additional statuses or transitions.)

---

## Enqueueing deliveries

When domain events (e.g. `ClaimStatusChangedEvent`, `EobGeneratedEvent`) are published, `WebhookEventSubscriber` subscribes and creates `WebhookDelivery` rows for each active endpoint, with `Status = 'Pending'` and `NextAttemptAt = UtcNow` (or similar). Payload is serialized to JSON and stored in `PayloadJson`. (Verify in code: `WebhookEventSubscriber` and event-to-payload mapping.)
