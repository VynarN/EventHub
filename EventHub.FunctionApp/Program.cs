using EventHub.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Avoid FunctionsApplication.CreateBuilder + ConfigureFunctionsWebApplication: both register worker defaults and
// break mixed HTTP (AspNetCore) + Service Bus triggers. See azure-functions-dotnet-worker#1947.
var host = Host.CreateDefaultBuilder(args)
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddEventHubCosmos(context.Configuration);
    })
    .Build();

host.Run();
