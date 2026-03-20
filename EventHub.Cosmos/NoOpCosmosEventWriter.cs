namespace EventHub.Cosmos;

/// <summary>Used when CosmosDb:ConnectionString is not configured (e.g. isolated unit tests).</summary>
public sealed class NoOpCosmosEventWriter : ICosmosEventWriter
{
    public Task UpsertAsync(CosmosEventDocument document, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
