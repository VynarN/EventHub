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
        if (string.IsNullOrWhiteSpace(connectionString))
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
