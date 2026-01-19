namespace WebhookGateway.Api.Options
{
    public class ServiceBusOptions
    {
        public string FullyQualifiedNamespace { get; init ; }
        public string TopicName { get; init; } = "t-webhooks";
    }
}
