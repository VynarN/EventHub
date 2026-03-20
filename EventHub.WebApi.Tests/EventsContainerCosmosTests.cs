using EventHub.Cosmos;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Xunit;

namespace EventHub.WebApi.Tests;

public class EventsContainerCosmosTests
{
    [SkippableFact]
    public async Task Events_container_supports_write_and_read_roundtrip()
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
            throw new SkipException("Cosmos emulator is not reachable; start `docker compose up cosmos-emulator` (or full stack) and use HTTPS endpoint https://localhost:8081.");
        }

        await CosmosInfrastructure
            .EnsureDatabaseAndEventsContainerAsync(client, settings, cts.Token)
            .ConfigureAwait(false);

        var id = Guid.NewGuid().ToString("N");
        var container = client.GetContainer(settings.DatabaseName, settings.ContainerName);
        var item = new TestDoc
        {
            DocumentId = id,
            PartitionId = id,
            Kind = "story-1-2-integration",
        };

        await container
            .CreateItemAsync(item, new PartitionKey(id), cancellationToken: cts.Token)
            .ConfigureAwait(false);

        var read = await container
            .ReadItemAsync<TestDoc>(id, new PartitionKey(id), cancellationToken: cts.Token)
            .ConfigureAwait(false);

        Assert.Equal("story-1-2-integration", read.Resource.Kind);
    }

    private sealed class TestDoc
    {
        [JsonProperty("id")]
        public string DocumentId { get; set; } = "";

        [JsonProperty("Id")]
        public string PartitionId { get; set; } = "";

        [JsonProperty("kind")]
        public string Kind { get; set; } = "";
    }
}
