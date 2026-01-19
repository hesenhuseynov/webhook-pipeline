namespace WebhookGateway.Api.Contracts
{
    public sealed record WebHookingestResult
    (bool Ok, string MessageId, string TenantId, string EventType
        );
}
