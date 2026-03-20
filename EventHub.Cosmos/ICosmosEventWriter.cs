namespace EventHub.Cosmos;

public interface ICosmosEventWriter
{
    Task UpsertAsync(CosmosEventDocument document, CancellationToken cancellationToken = default);
}
