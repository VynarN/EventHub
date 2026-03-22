using EventHub.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// ASP.NET Core integration (ConfigureFunctionsWebApplication) is optional and has caused 0 functions / Custom
// metadata issues when combined with Service Bus. Use worker defaults + HttpRequestData HTTP triggers only.
var host = Host.CreateDefaultBuilder(args)
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddEventHubCosmos(context.Configuration);

        // Log connection configuration state at startup so Application Insights shows whether
        // Key Vault references resolved. Unresolved references appear as "@Microsoft.KeyVault(...)"
        // literals — a non-empty but invalid value that previously crashed the worker (0 functions).
        services.AddHostedService<StartupConfigLogger>();
    })
    .Build();

host.Run();

internal sealed class StartupConfigLogger(ILogger<StartupConfigLogger> logger, IConfiguration configuration)
    : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        LogSetting("ServiceBusConnection", configuration["ServiceBusConnection"], "Endpoint=sb://");
        LogSetting("CosmosDb:ConnectionString", configuration["CosmosDb:ConnectionString"], "AccountEndpoint=");
        LogSetting("APPLICATIONINSIGHTS_CONNECTION_STRING", configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"], "InstrumentationKey=");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private void LogSetting(string name, string? value, string expectedPrefix)
    {
        if (string.IsNullOrWhiteSpace(value))
            logger.LogWarning("Startup config: {Setting} is empty or missing — using no-op fallback", name);
        else if (value.StartsWith("@Microsoft.KeyVault", StringComparison.OrdinalIgnoreCase))
            logger.LogError("Startup config: {Setting} is an unresolved Key Vault reference — Key Vault access policy may not be applied yet", name);
        else if (!value.Contains(expectedPrefix, StringComparison.OrdinalIgnoreCase))
            logger.LogWarning("Startup config: {Setting} does not look like a valid value (expected prefix '{Prefix}')", name, expectedPrefix);
        else
            logger.LogInformation("Startup config: {Setting} is configured", name);
    }
}
