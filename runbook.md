# Runbook

## Local Run

1) Ensure Azure CLI is logged in:
- az login

2) Start WebhookGateway.Api
- dotnet run

3) Start WebhookProcessor.Function
- func start

4) Send test webhook via HTTP file:
- WebhookGateway.Api.http

Expected:
- Message published to Service Bus
- AuditProcessor executes (sub-audit)
- BillingProcessor executes when eventType matches billing filter
- BillingWorker executes after queue message is created

---

## Azure Verification

### Service Bus
- Confirm topic receives messages
- Confirm subscription `sub-billing` filter is correct:
  - eventType LIKE 'invoice.%'
- Confirm `sub-audit` has no filter (or TrueFilter)

### Storage Queue
- Confirm queue exists: q-billing-jobs
- Verify messages via Azure Portal or Storage Explorer

### Cosmos DB
- Verify audit documents are being inserted
