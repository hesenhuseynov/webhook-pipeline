using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebhookProcessor.Function
{
    public sealed  class BillingJob
    {
        public string MessageId  { get; init; }

        public string TenantId { get; init ; }

        public string EventType { get; init; }

        public string Body { get; init; }

        public DateTime EnqueuedUtc  { get; init ; }
    }
}
