using Azure;
using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Cosmos.Serialization.HybridRow.Layouts;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.Win32.SafeHandles;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using WebhookProcessor.Function.Helpers;

namespace WebhookProcessor.Function.Processors
{
    public sealed class AuditProcessor
    {
        private readonly CosmosClient _cosmos;
        private readonly ILogger<AuditProcessor> _logger;
        private const string DbName = "webhooks";
        private const string ContainerName = "events";
        
        public AuditProcessor(CosmosClient cosmos,ILogger<AuditProcessor> logger)
        {
            _cosmos = cosmos;
            _logger = logger;  
        }

        [Function("AuditProcessor")]
        public async Task Run(
            [ServiceBusTrigger("t-webhooks","sub-audit",Connection ="SB")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions,
            CancellationToken ct)
        {
            var body = message.Body.ToString();
            string tenantId; 
            string eventType;
            try
            {
                tenantId = SbProps.GetTenantId(message);
                eventType = SbProps.GetEventType(message);
            }
             
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                                "AUDIT: Missing required application property. MessageId={MessageId}",
                                message.MessageId);

                await messageActions.DeadLetterMessageAsync(
                    message,
                    deadLetterReason: "MissingRequiredProperty",
                    deadLetterErrorDescription: "eventTyep application property  is required",
                    cancellationToken: ct
                    );

                return;  
            }

            
            _logger.LogInformation(
                     "AUDIT: MessageId={MessageId} eventType={EventType} tenantId={TenantId} DeliveryCount={DeliveryCount}",
                     message.MessageId, eventType, tenantId, message.DeliveryCount);

            var container = _cosmos.GetContainer(DbName, ContainerName);

            var doc = new AuditEventDocument
            {
                id = message.MessageId,
                tenantId = tenantId,
                eventType = eventType,
                receivedAtUtc = DateTime.UtcNow,
                raw = body,
                source = "webhooks-dev"
            };

            try
            {
                await container.CreateItemAsync(doc, new PartitionKey(doc.tenantId), cancellationToken: ct);
            }
            catch (CosmosException ex  ) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                _logger.LogWarning("Audit: Xe xe xe Dupliate ignored .MessageId={MessageId}", message.MessageId);
            }

            await messageActions.CompleteMessageAsync(message, ct);

            _logger.LogWarning("Audit: Completed  id= {id}", doc.id);
        }
    }
}
