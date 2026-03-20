using EventHub.Cosmos;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using MsOptions = Microsoft.Extensions.Options;
using Xunit;

namespace EventHub.WebApi.Tests;

public class CosmosEventWriterIntegrationTests
{
    [SkippableFact]
    public async Task Upsert_persists_event_document_for_list_queries()
    {
        var config = TestConfiguration.Load();
        var settings = config.GetSection(CosmosDbSettings.SectionName).Get<CosmosDbSettings>();
        Skip.If(settings is null || string.IsNullOrWhiteSpace(settings.ConnectionString));

        using var client = CosmosInfrastructure.CreateClient(settings);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        try
        {
            await client.ReadAccountAsync().ConfigureAwait(false);
        }
        catch
        {
            throw new SkipException("Cosmos emulator is not reachable; start `docker compose up cosmos-emulator` (or full stack).");
        }

        await CosmosInfrastructure
            .EnsureDatabaseAndEventsContainerAsync(client, settings, cts.Token)
            .ConfigureAwait(false);

        var id = Guid.NewGuid();
        var doc = CosmosEventDocument.FromEvent(
            id,
            "user-integration",
            "click",
            "from writer test",
            DateTime.UtcNow);

        var writer = new CosmosEventWriter(client, MsOptions.Options.Create(settings), NullLogger<CosmosEventWriter>.Instance);
        await writer.UpsertAsync(doc, cts.Token).ConfigureAwait(false);

        var container = client.GetContainer(settings.DatabaseName, settings.ContainerName);
        var read = await container
            .ReadItemAsync<CosmosEventDocument>(id.ToString("D"), new PartitionKey(doc.PartitionId), cancellationToken: cts.Token)
            .ConfigureAwait(false);

        Assert.Equal("user-integration", read.Resource.UserId);
        Assert.Equal("click", read.Resource.Type);
        Assert.Equal(id, read.Resource.Id);
        Assert.Equal(id.ToString("D"), read.Resource.PartitionId);
    }
}
