namespace WebhookGateway.Api.Options
{
    public class WebHookAuthOptions
    {
        public string IngressKey { get; init; }

        public string HeaderName { get; init; } = "x-webhook-key";
     }
}

