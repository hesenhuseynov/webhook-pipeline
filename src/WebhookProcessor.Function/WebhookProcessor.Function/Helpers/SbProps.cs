using Azure.Messaging.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;

namespace WebhookProcessor.Function.Helpers
{
    internal static  class SbProps
    {
        private const string TenantIdKey = "tenantId";
        private const string EventTypeKey = "eventType";
        private static string? GetOptional(ServiceBusReceivedMessage message, string key)
        => message.ApplicationProperties.TryGetValue(key, out var v) ? v?.ToString() : null;
        private static string Require(ServiceBusReceivedMessage message, string key)
        {
            var value = GetOptional(message, key); 

            if(string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"Application property '{key}' is required and can't be empty.  "); 
            }
            return   value; 
         }
        public static string GetTenantId(ServiceBusReceivedMessage message)
            => Require(message, TenantIdKey);

        public static string GetEventType(ServiceBusReceivedMessage message)
            => Require(message, EventTypeKey);
    }
}
