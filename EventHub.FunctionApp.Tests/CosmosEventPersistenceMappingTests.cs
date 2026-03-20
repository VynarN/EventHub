using EventHub.Cosmos;
using EventHub.FunctionApp.Models;
using Xunit;

namespace EventHub.FunctionApp.Tests;

public class CosmosEventPersistenceMappingTests
{
    [Fact]
    public void SerializeEventType_matches_web_api_publisher_casing()
    {
        Assert.Equal("pageView", EventDeserialization.SerializeEventType(EventType.PageView));
        Assert.Equal("click", EventDeserialization.SerializeEventType(EventType.Click));
        Assert.Equal("purchase", EventDeserialization.SerializeEventType(EventType.Purchase));
    }

    [Fact]
    public void CosmosEventDocument_FromEvent_sets_partition_id_and_utc_createdAt()
    {
        var id = Guid.Parse("a1b2c3d4-e5f6-4789-a012-3456789abcde");
        var created = new DateTime(2026, 3, 19, 12, 0, 0, DateTimeKind.Unspecified);
        var doc = CosmosEventDocument.FromEvent(id, "u1", "pageView", "desc", created);

        Assert.Equal(id, doc.Id);
        Assert.Equal(id.ToString("D"), doc.PartitionId);
        Assert.Equal("u1", doc.UserId);
        Assert.Equal("pageView", doc.Type);
        Assert.Equal("desc", doc.Description);
        Assert.Equal(DateTimeKind.Utc, doc.CreatedAt.Kind);
    }
}
