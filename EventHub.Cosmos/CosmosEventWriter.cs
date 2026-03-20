using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EventHub.Cosmos;

public sealed class CosmosEventWriter(
    CosmosClient cosmosClient,
    IOptions<CosmosDbSettings> cosmosOptions,
    ILogger<CosmosEventWriter> logger) : ICosmosEventWriter
{
    public async Task UpsertAsync(CosmosEventDocument document, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        if (string.IsNullOrEmpty(document.PartitionId))
            throw new ArgumentException("PartitionId is required for Cosmos partition key /Id.", nameof(document));

        var settings = cosmosOptions.Value;
        var container = cosmosClient.GetContainer(settings.DatabaseName, settings.ContainerName);
        try
        {
            await container
                .UpsertItemAsync(document, new PartitionKey(document.PartitionId), cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to upsert event to Cosmos DB. EventId={EventId}", document.Id);
            throw;
        }
    }
}
