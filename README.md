
**Qeyd:** `sub-users` üçün çıxışı “future actions” kimi qoydum — çünki real həyatda bu subscription adətən “user.created / user.updated” kimi event-lərə gedir və downstream integration olur. Səndə hazırda nə edirsənsə, ora uyğunlaşdırarıq.

---

## ✅ README.md (FULL – audit + users + billing, professional)

> Fayl: `README.md`

```md
# Webhook Pipeline (API → Service Bus Topic → Azure Functions → Cosmos DB + Storage Queue)

This repository demonstrates an event-driven webhook processing pipeline on Azure using:
- Ingress API (WebhookGateway)
- Service Bus Topic fan-out + routing (SQL Filters on subscriptions)
- Azure Functions (.NET 8 isolated) processors
- Cosmos DB persistence for audit events
- Storage Queue for billing jobs + QueueTrigger worker

The solution is designed to reflect production-style separation of concerns:
ingress validation, message routing, asynchronous processing, and durable job execution.

---

## High-Level Architecture

1. `WebhookGateway.Api` receives an HTTP webhook request.
2. The API validates the ingress key, extracts routing information, and publishes a message to
   Service Bus Topic `t-webhooks`.
3. Subscriptions route messages using Application Properties (not message body).
4. Azure Functions consume from subscriptions and execute dedicated workloads.

### Subscriptions and Flows

**Audit flow**
- `sub-audit` → `AuditProcessor` → Cosmos DB (audit events)

**Users flow**
- `sub-users` → `UserProcessor` → user-domain processing (extensible)

**Billing flow**
- `sub-billing` (SQL filter: `eventType LIKE 'invoice.%'`)
  → `BillingProcessor` → Storage Queue `q-billing-jobs`
  → `BillingWorker` (QueueTrigger)

---

## Message Contract

### Service Bus Message

**Application Properties (required)**
- `tenantId`
- `eventType`

**Body**
- original webhook payload JSON

### Billing Job (Storage Queue Message)

Billing jobs stored in `q-billing-jobs` contain:
- messageId
- tenantId
- eventType
- body
- enqueuedAtUtc

---

## Local Development

### Prerequisites
- .NET 8 SDK
- Azure Functions Core Tools
- Azure CLI logged-in (DefaultAzureCredential)

### Run
1. Run `WebhookGateway.Api`
2. Run `WebhookProcessor.Function`

### Test
Use `WebhookGateway.Api.http` and send:
- `x-ingress-key`
- `x-tenant-id`
- `x-event-type`

---

## Azure Resources

- Service Bus Namespace
  - Topic: `t-webhooks`
  - Subscriptions:
    - `sub-audit`
    - `sub-users`
    - `sub-billing`

- Cosmos DB account (audit persistence)

- Storage Account
  - Queue: `q-billing-jobs`

---

## Security Notes

- Secrets are not committed.
- Local configuration is stored in `local.settings.json` (ignored by Git).
- In Azure, Managed Identity + RBAC is used for Service Bus, Cosmos DB, and Storage access.

---

## Operational Notes (Important)

- Routing depends on exact Application Property names.
  A typo in SQL Filter property names (e.g. `evenType` instead of `eventType`)
  will silently prevent messages from reaching that subscription.
