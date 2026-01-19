# Troubleshooting

## 1) BillingProcessor does not run
Cause:
- Subscription filter mismatch
- Most common: property name typo in SQL Filter
  Example: evenType vs eventType

Fix:
- Ensure Application Property key is exactly `eventType`
- Ensure SQL Filter references the exact key

---

## 2) BillingProcessor runs but queue message does not appear
Cause:
- Storage RBAC missing
- Error usually: 403 AuthorizationPermissionMismatch

Fix:
- Assign correct role to identity that is used by the Function:
  - Storage Queue Data Contributor
- Wait a few minutes for RBAC propagation

---

## 3) Queue looks empty in Azure Portal but function says it enqueued
Cause:
- Portal view authentication mismatch
- Function uses Managed Identity, portal UI might be using Access Key view

Fix:
- Prefer Storage Explorer or `az storage queue message peek`
- Verify correct storage account and queue name

---

## 4) Messages not reaching a subscription
Cause:
- SQL Filter conditions not matching
- Property names are case-sensitive

Fix:
- Inspect message Application Properties
- Validate filter expression using a known eventType
