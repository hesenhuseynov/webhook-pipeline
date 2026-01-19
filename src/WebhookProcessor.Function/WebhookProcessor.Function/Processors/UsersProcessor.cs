using Azure.Messaging.ServiceBus;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.Azure.Cosmos.Core;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebhookProcessor.Function.Helpers;

namespace WebhookProcessor.Function.Processors
{
    public class UsersProcessor
    {
        private readonly ILogger<UsersProcessor> _logger;
        public UsersProcessor(ILogger<UsersProcessor> logger)
        {
            _logger = logger;
        }

        [Function("UserProcessor")]
        public async Task Run(
            [ServiceBusTrigger("t-webhooks", "sub-users",Connection ="SB")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions,
            CancellationToken ct
            )
        {
            var body = message.Body.ToString();
            string tenantId;
            string eventType;
            try
            {
                tenantId = SbProps.GetTenantId(message);
                eventType = SbProps.GetEventType(message); 
            }

            catch (Exception ex )
            {
                _logger.LogWarning(ex, "Users: missing eventType    MessageId= {MessageId} ", message.MessageId);

                await messageActions.DeadLetterMessageAsync(
                    message,
                    deadLetterReason: "MissingRequiredProperty",
                    deadLetterErrorDescription: " tenantId and  eventType  application  property is reuqired",
                    cancellationToken: ct
                    );
                return; 
            }
            
            _logger.LogInformation(
           "USERS: MessageId={MessageId} eventType={EventType} tenantId={TenantId} DeliveryCount={DeliveryCount}",
           message.MessageId, eventType, tenantId, message.DeliveryCount);

            try
            {
                using var json = JsonDocument.Parse(body);
                var userId = json.RootElement.TryGetProperty("userId", out var p) ? p.GetString() : null;

                _logger.LogInformation("Users:Parsed userId={userId}", userId);  
            }

            catch (Exception ex  )
            {   
                _logger.LogWarning("Userus: Invalid Json body . MessageId={MessageId} body={Body}", message.MessageId, body);  
            }

            await messageActions.CompleteMessageAsync(message, ct);

            _logger.LogInformation("Users:Completed  ,MessageId={MessageId}", message.MessageId);
        }
    }
}
