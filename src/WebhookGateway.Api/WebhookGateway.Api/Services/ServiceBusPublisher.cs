using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using WebhookGateway.Api.Options;

namespace WebhookGateway.Api.Services
{
    public sealed class  ServiceBusPublisher
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusOptions _opt;
        public ServiceBusPublisher(ServiceBusClient client, IOptions<ServiceBusOptions> opt)
        {
            _client = client;
            _opt = opt.Value;
        }
        
        public async Task<string> PublishAsync(
            string tenantId, string eventType, string body, CancellationToken ct
            )
        {
            var sender = _client.CreateSender(_opt.TopicName);
            var messageId = Guid.NewGuid().ToString("N");
            
            var msg = new ServiceBusMessage(body)
            {
                MessageId = messageId,
                Subject = eventType
            };
            msg.ApplicationProperties["tenantId"] = tenantId;
            msg.ApplicationProperties["eventType"] = eventType;
            await sender.SendMessageAsync(msg, ct);
            return messageId;
        }
    }
}
