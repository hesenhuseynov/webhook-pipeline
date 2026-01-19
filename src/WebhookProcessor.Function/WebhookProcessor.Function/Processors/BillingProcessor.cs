using Azure.Messaging.ServiceBus;
using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebhookProcessor.Function.Helpers;

namespace WebhookProcessor.Function.Processors
{
    internal sealed class BillingProcessor
    {
        private readonly QueueServiceClient _queueServiceClient;
        private readonly ILogger<BillingProcessor> _logger;

        private const string QueueName = "q-billing-jobs";
        public BillingProcessor(
            QueueServiceClient queueServiceClient,
            ILogger<BillingProcessor> logger)
        {
            _queueServiceClient = queueServiceClient;
            _logger = logger;
        }

        [Function("BillingProcessor")]
        public async Task Run(
            [ServiceBusTrigger(
            "t-webhooks",
            "sub-billing",
            Connection = "SB")]
        ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions,
            CancellationToken ct)
        {
            string tenantId;
            string eventType;

            try
            {
                tenantId = SbProps.GetTenantId(message);
                eventType = SbProps.GetEventType(message);
            }

            catch (Exception ex)
            {
                _logger.LogWarning(ex, "BILLING: Missing required props. MessageId={MessageId}", message.MessageId);

                await messageActions.DeadLetterMessageAsync(
                          message,
                          deadLetterReason: "MissingRequiredProperty",
                          deadLetterErrorDescription: "tenantId and eventType application properties are required",
                          cancellationToken: ct);
                return;
            }

            var body = message.Body.ToString();

            var queueClient = _queueServiceClient.GetQueueClient(QueueName);
            await queueClient.CreateIfNotExistsAsync(cancellationToken: ct);

            var payload = new
            {
                messageId = message.MessageId,
                tenantId,
                eventType,
                body,
                enqueuedAtUtc = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(payload);  
            //var base64 =Convert.ToBase64String(Encoding.UTF8.GetBytes(json));


            try
            {
                await queueClient.SendMessageAsync(json,cancellationToken: ct);
            }
            catch (Exception ex )
            {
                _logger.LogError(ex, "BILLIGN:Failed to queue . MessageId ={MessageId}", message.MessageId);
                throw;
            }


            await messageActions.CompleteMessageAsync(message, ct);
            _logger.LogInformation(
            "BILLING: Enqueued job. MessageId={MessageId} tenantId={TenantId} eventType={EventType}",
            message.MessageId, tenantId, eventType);
        }
    }
}
