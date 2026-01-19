using Azure.Identity;
using Azure.Storage.Queues;
using Microsoft.AspNetCore.Components;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Client.Extensions.Msal;
using System.Runtime.Intrinsics.X86;

var builder = FunctionsApplication.CreateBuilder(args);

var endpoint = builder.Configuration["COSMOS:accountEndpoint"];

if (string.IsNullOrWhiteSpace(endpoint))
{
    throw new InvalidOperationException("COSMOS:accountEndpoint  is missing");
}
var credential = new DefaultAzureCredential();

var cosmosClient = new CosmosClient(endpoint, credential, new CosmosClientOptions
{
    ApplicationName = "WebhookProcessor.Dev"
});

builder.Services.AddSingleton(cosmosClient);

var queueServiceUri = builder.Configuration["STORAGE:queueServiceUri"];
if(string.IsNullOrWhiteSpace(queueServiceUri))
{
    throw new InvalidOperationException("STORAGE:queueServiceUri is missing");
 }
var queueServiceClient = new QueueServiceClient(new Uri(queueServiceUri), credential,new QueueClientOptions { MessageEncoding=QueueMessageEncoding.Base64});
builder.Services.AddSingleton(queueServiceClient);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Build().Run();
