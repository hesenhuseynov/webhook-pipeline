# Architecture

## Components

### WebhookGateway.Api
Ingress layer that accepts webhooks and forwards them into the event pipeline.

Responsibilities:
- Validate ingress key
- Read request headers for routing:
  - `x-tenant-id`
  - `x-event-type`
- Publish to Service Bus Topic `t-webhooks` with Application Properties:
  - tenantId
  - eventType
- Body is the original webhook JSON payload

The API does not execute business logic. It only validates, normalizes, and publishes.

---

### Service Bus Topic (t-webhooks)
Topic provides fan-out (multiple consumers) and routing (filters).

Subscriptions:
- `sub-audit`: no filter, receives all events
- `sub-users`: filtered by eventType (user-related event patterns)
- `sub-billing`: filtered by eventType for billing-related events
  - example: `eventType LIKE 'invoice.%'`

Routing uses Application Properties to avoid parsing and filtering message bodies.

---

### WebhookProcessor.Function (.NET 8 isolated)

#### AuditProcessor
- Trigger: Service Bus subscription `sub-audit`
- Writes audit event documents to Cosmos DB

#### UserProcessor
- Trigger: Service Bus subscription `sub-users`
- Handles user-domain events (extensible / evolving)

#### BillingProcessor
- Trigger: Service Bus subscription `sub-billing`
- Validates required properties
- Enqueues a job into Storage Queue `q-billing-jobs`

#### BillingWorker
- Trigger: Storage Queue `q-billing-jobs`
- Processes billing jobs asynchronously

---

## Identity and Access

- Local: DefaultAzureCredential (Azure CLI / VS / az login)
- Azure: Managed Identity + RBAC
  - Service Bus: Azure Service Bus Data Receiver/Sender (as needed)
  - Cosmos DB: Cosmos DB Built-in Data Contributor (data plane)
  - Storage Queue: Storage Queue Data Contributor
