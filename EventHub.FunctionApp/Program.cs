using EventHub.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// ASP.NET Core integration (ConfigureFunctionsWebApplication) is optional and has caused 0 functions / Custom
// metadata issues when combined with Service Bus. Use worker defaults + HttpRequestData HTTP triggers only.
var host = Host.CreateDefaultBuilder(args)
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddEventHubCosmos(context.Configuration);
    })
    .Build();

host.Run();
