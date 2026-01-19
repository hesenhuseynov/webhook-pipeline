using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace WebhookProcessor.Function.Workers
{
    public sealed class BillingWorker
    {
        private readonly ILogger<BillingWorker> _logger;

        public BillingWorker(ILogger<BillingWorker> logger)
        {
            _logger = logger;
        }

        [Function("BillingWorker")]
        public async Task Run(
            [QueueTrigger("q-billing-jobs", Connection = "BILLINGQ")] string queueMessage,
            FunctionContext context,
            CancellationToken ct
            )
        {
            BillingJob? job = null;

            try
            {
                job = JsonSerializer.Deserialize<BillingJob>(queueMessage, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (job is null ||
                                string.IsNullOrWhiteSpace(job.MessageId) ||
                                string.IsNullOrWhiteSpace(job.TenantId) ||
                                string.IsNullOrWhiteSpace(job.EventType))
                {
                    throw new InvalidOperationException("Billing job payload is invalid or missing required fields.");
                }


                _logger.LogInformation(
                  "BILLING-WORKER: Start job MessageId={MessageId} TenantId={TenantId} EventType={EventType}",
                  job.MessageId, job.TenantId, job.EventType);

                await Task.Delay(200, ct);

                _logger.LogInformation(
                    "BILLING-WORKER: Completed job MessageId={MessageId}",
                    job.MessageId);

            }

            catch (Exception ex)
            {
                _logger.LogError(ex,
                   "BILLING-WORKER: Failed job. PayloadSnippet={Payload}",
                   SafeSnippet(queueMessage));

                throw;
            }

        }


        private static string SafeSnippet(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Length <= 512 ? s : s[..512];
        }
    }
}

