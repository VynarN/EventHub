using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EventHub.Cosmos;

public static class CosmosInfrastructure
{
    public static Task EnsureDatabaseAndEventsContainerAsync(
        CosmosClient client,
        CosmosDbSettings settings,
        CancellationToken cancellationToken = default) =>
        EnsureDatabaseAndEventsContainerAsync(
            client,
            settings,
            Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance,
            cancellationToken);

    public static CosmosClient CreateClient(CosmosDbSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        if (string.IsNullOrWhiteSpace(settings.ConnectionString))
            throw new InvalidOperationException("CosmosDb:ConnectionString is not configured.");

        var options = new CosmosClientOptions
        {
            ConnectionMode = ConnectionMode.Gateway,
            ApplicationName = "EventHub",
        };

        if (settings.DisableServerCertificateValidation)
        {
            options.HttpClientFactory = () => new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
            });
            options.LimitToEndpoint = true;
        }

        return new CosmosClient(settings.ConnectionString, options);
    }

    public static async Task EnsureDatabaseAndEventsContainerAsync(
        CosmosClient client,
        CosmosDbSettings settings,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(logger);

        var dbName = settings.DatabaseName;
        var containerName = settings.ContainerName;
        var pkPath = string.IsNullOrWhiteSpace(settings.PartitionKeyPath) ? "/Id" : settings.PartitionKeyPath;

        logger.LogInformation("Ensuring Cosmos database {Database} exists", dbName);
        var dbResponse = await client
            .CreateDatabaseIfNotExistsAsync(dbName, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        logger.LogInformation("Ensuring container {Container} with partition key {PartitionKeyPath}", containerName, pkPath);
        await dbResponse.Database
            .CreateContainerIfNotExistsAsync(
                new ContainerProperties(containerName, pkPath),
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }
}

public sealed class CosmosDbInitializationHostedService : IHostedService
{
    private readonly CosmosClient _client;
    private readonly IOptions<CosmosDbSettings> _options;
    private readonly ILogger<CosmosDbInitializationHostedService> _logger;

    public CosmosDbInitializationHostedService(
        CosmosClient client,
        IOptions<CosmosDbSettings> options,
        ILogger<CosmosDbInitializationHostedService> logger)
    {
        _client = client;
        _options = options;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var settings = _options.Value;
        if (string.IsNullOrWhiteSpace(settings.ConnectionString))
        {
            _logger.LogWarning("CosmosDb:ConnectionString is empty; skipping database and container provisioning.");
            return;
        }

        const int maxAttempts = 30;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await CosmosInfrastructure
                    .EnsureDatabaseAndEventsContainerAsync(_client, settings, _logger, cancellationToken)
                    .ConfigureAwait(false);
                return;
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                _logger.LogWarning(
                    ex,
                    "Cosmos database provisioning attempt {Attempt}/{MaxAttempts} failed; retrying in 2s.",
                    attempt,
                    maxAttempts);
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken).ConfigureAwait(false);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
