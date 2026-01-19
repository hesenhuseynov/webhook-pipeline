
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using WebhookGateway.Api.Contracts;
using WebhookGateway.Api.Options;
using WebhookGateway.Api.Services;

namespace WebhookGateway.Api
{
     internal class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddOptions<ServiceBusOptions>()
                 .Bind(builder.Configuration.GetSection("SB"))
                 .Validate(o => !string.IsNullOrWhiteSpace(o.FullyQualifiedNamespace), "Sb:FullyQualifedNamespace is required")
                 .ValidateOnStart();

            builder.Services.AddOptions<WebHookAuthOptions>()
                .Bind(builder.Configuration.GetSection("WEBHOOK"))
                .Validate(o => !string.IsNullOrWhiteSpace(o.IngressKey), "WEBHOOK:IngressKey is required")
                .ValidateOnStart();
           
            builder.Services.AddSingleton(sp =>
            {
                var sb = sp.GetRequiredService<IOptions<ServiceBusOptions>>().Value;

                var credential = new DefaultAzureCredential(); 
               
                return new ServiceBusClient(sb.FullyQualifiedNamespace, credential);
            });

            builder.Services.AddSingleton<ServiceBusPublisher>();
            builder.Services.AddSwaggerGen();
            builder.Services.AddEndpointsApiExplorer();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            //app.UseAuthorization();
            app.MapGet("/health", () => Results.Ok(new { ok = true }));

            app.MapPost("/webhooks", async (
                HttpRequest request,
                ServiceBusPublisher publisher,
                IOptions<WebHookAuthOptions> authOpt,
                CancellationToken ct) =>
            {
                var auth = authOpt.Value;
            
                if (!request.Headers.TryGetValue(auth.HeaderName, out var key) || key != auth.IngressKey)
                    return Results.Unauthorized();
                var tenantId = request.Headers["x-tenant-id"].ToString();
                if (string.IsNullOrWhiteSpace(tenantId)) tenantId = "dev";
                var eventType = request.Headers["x-event-type"].ToString();
                if (string.IsNullOrWhiteSpace(eventType)) eventType = "audit";
                using var reader = new StreamReader(request.Body);
                var body = await reader.ReadToEndAsync(ct);
                if (string.IsNullOrWhiteSpace(body)) body = "{}";
                var messageId = await publisher.PublishAsync(tenantId, eventType, body, ct);
                return Results.Ok(new WebHookingestResult(true, messageId, tenantId, eventType));
            });

            //app.MapControllers();

            app.Run();
        }
    }
}
