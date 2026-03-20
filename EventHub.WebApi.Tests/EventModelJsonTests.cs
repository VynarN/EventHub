using System.Text.Json;
using System.Text.Json.Serialization;
using EventHub.WebApi.Models;
using EventHub.WebApi.Models.EMs;
using EventHub.WebApi.Models.VMs;
using Xunit;

namespace EventHub.WebApi.Tests;

public class EventModelJsonTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    [Fact]
    public void Event_serializes_property_names_as_camelCase()
    {
        var id = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
        var created = new DateTime(2026, 3, 19, 12, 0, 0, DateTimeKind.Utc);
        var evt = new Event
        {
            Id = id,
            UserId = "user-1",
            Type = EventType.PageView,
            Description = "Home",
            CreatedAt = created,
        };

        var json = JsonSerializer.Serialize(evt, JsonOptions);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        Assert.True(root.TryGetProperty("id", out var idEl));
        Assert.Equal(id, idEl.GetGuid());
        Assert.True(root.TryGetProperty("userId", out _));
        Assert.True(root.TryGetProperty("type", out var typeEl));
        Assert.Equal("pageView", typeEl.GetString());
        Assert.True(root.TryGetProperty("description", out _));
        Assert.True(root.TryGetProperty("createdAt", out _));
    }

    [Fact]
    public void EventCreationDto_roundtrips_json_with_camelCase_and_string_enum()
    {
        const string json = """{"userId":"u42","type":"click","description":"cta"}""";

        var dto = JsonSerializer.Deserialize<EventCreationRequest>(json, JsonOptions);
        Assert.NotNull(dto);
        Assert.Equal("u42", dto.UserId);
        Assert.Equal(EventType.Click, dto.Type);
        Assert.Equal("cta", dto.Description);

        var roundTrip = JsonSerializer.Serialize(dto, JsonOptions);
        using var doc = JsonDocument.Parse(roundTrip);
        var root = doc.RootElement;
        Assert.Equal("u42", root.GetProperty("userId").GetString());
        Assert.Equal("click", root.GetProperty("type").GetString());
        Assert.Equal("cta", root.GetProperty("description").GetString());
    }

    [Fact]
    public void PagedEventsResponse_serializes_items_array_for_clients_and_load_tests()
    {
        var page = new PagedEventsResponse
        {
            Items = [new Event { Id = Guid.NewGuid(), UserId = "u", Type = EventType.PageView, Description = "x", CreatedAt = DateTime.UtcNow }],
            PageNumber = 1,
            PageSize = 100,
            TotalCount = 1,
        };

        var json = JsonSerializer.Serialize(page, JsonOptions);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        Assert.True(root.TryGetProperty("items", out var items));
        Assert.Equal(JsonValueKind.Array, items.ValueKind);
        Assert.True(root.TryGetProperty("pageNumber", out _));
        Assert.True(root.TryGetProperty("pageSize", out _));
        Assert.True(root.TryGetProperty("totalCount", out _));
    }
}
