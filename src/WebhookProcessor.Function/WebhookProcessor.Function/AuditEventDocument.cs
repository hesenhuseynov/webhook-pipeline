using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace WebhookProcessor.Function
{
    public sealed class AuditEventDocument
    {
        public string id { get; set; } = Guid.NewGuid().ToString("N");
        public string tenantId { get; set; } = default!;
        public string eventType { get; set; } = "unkwon";
        public DateTime receivedAtUtc { get; set; } = DateTime.UtcNow;
        public string raw { get; set; } = string.Empty;
        public string source { get; set; } = "webhooks-dev";
    }
  
}
