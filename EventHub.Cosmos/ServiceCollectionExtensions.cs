using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventHub.Cosmos;

public static class ServiceCollectionExtensions
{
    /// <summary>Registers <see cref="CosmosClient"/> and a hosted service that creates the EventHub database and Events container if missing.</summary>
    public static IServiceCollection AddEventHubCosmos(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CosmosDbSettings>(configuration.GetSection(CosmosDbSettings.SectionName));

        var connectionString = configuration["CosmosDb:ConnectionString"];

        // Reject unresolved Key Vault reference literals (e.g. "@Microsoft.KeyVault(SecretUri=...)").
        // They are non-empty strings but not valid Cosmos connection strings; passing one to
        // CosmosClient throws ArgumentException, crashing the worker process and causing 0 functions loaded.
        var isValid = !string.IsNullOrWhiteSpace(connectionString)
            && connectionString.Contains("AccountEndpoint=", StringComparison.OrdinalIgnoreCase);

        if (!isValid)
        {
            services.AddSingleton<ICosmosEventWriter, NoOpCosmosEventWriter>();
            return services;
        }

        services.AddSingleton(sp =>
        {
            var settings = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<CosmosDbSettings>>().Value;
            return CosmosInfrastructure.CreateClient(settings);
        });

        services.AddSingleton<ICosmosEventWriter, CosmosEventWriter>();
        services.AddHostedService<CosmosDbInitializationHostedService>();

        return services;
    }
}
