using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using EventHub.WebApi.Data;
using EventHub.WebApi.Models;
using EventHub.WebApi.Models.EMs;
using EventHub.WebApi.Models.VMs;
using Xunit;

namespace EventHub.WebApi.Tests;

public class EventsGetTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    [Fact]
    public async Task Get_events_returns_200_with_empty_items_and_metadata()
    {
        factory.CapturingEventListReader.Reset();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/events");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PagedEventsResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.Empty(body!.Items);
        Assert.Equal(1, body.PageNumber);
        Assert.Equal(EventCosmosQueryBuilder.DefaultPageSize, body.PageSize);
        Assert.Equal(0, body.TotalCount);

        Assert.Single(factory.CapturingEventListReader.Calls);
        var call = factory.CapturingEventListReader.Calls[0];
        Assert.Equal(1, call.PageNumber);
        Assert.Equal(EventCosmosQueryBuilder.DefaultPageSize, call.PageSize);
        Assert.Null(call.TypeFilter);
        Assert.Null(call.UserIdFilter);
        Assert.Null(call.CreatedFromUtc);
        Assert.Null(call.CreatedToUtc);
    }

    [Fact]
    public async Task Get_events_passes_pagination_and_filters_to_reader()
    {
        factory.CapturingEventListReader.Reset();
        var client = factory.CreateClient();

        var from = new DateTime(2026, 1, 10, 12, 0, 0, DateTimeKind.Utc);
        var to = new DateTime(2026, 1, 20, 12, 0, 0, DateTimeKind.Utc);
        await client.GetAsync(
            "/api/events?pageNumber=2&pageSize=10&type=click&userId=user-1" +
            "&createdFrom=2026-01-10T12:00:00.0000000Z&createdTo=2026-01-20T12:00:00.0000000Z");

        Assert.Single(factory.CapturingEventListReader.Calls);
        var call = factory.CapturingEventListReader.Calls[0];
        Assert.Equal(2, call.PageNumber);
        Assert.Equal(10, call.PageSize);
        Assert.Equal("click", call.TypeFilter);
        Assert.Equal("user-1", call.UserIdFilter);
        Assert.Equal(from, call.CreatedFromUtc);
        Assert.Equal(to, call.CreatedToUtc);
    }

    [Fact]
    public async Task Get_events_returns_reader_payload_and_camelCase_json()
    {
        factory.CapturingEventListReader.Reset();
        var id = Guid.NewGuid();
        factory.CapturingEventListReader.NextResponse = new PagedEventsResponse
        {
            Items =
            [
                new Event
                {
                    Id = id,
                    UserId = "u",
                    Type = EventType.PageView,
                    Description = "d",
                    CreatedAt = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc),
                },
            ],
            PageNumber = 3,
            PageSize = 15,
            TotalCount = 42,
        };

        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/events");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var raw = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(raw);
        var root = doc.RootElement;
        Assert.True(root.TryGetProperty("items", out var itemsEl));
        Assert.Equal(JsonValueKind.Array, itemsEl.ValueKind);
        Assert.Single(itemsEl.EnumerateArray());
        Assert.True(root.TryGetProperty("pageNumber", out _));
        Assert.True(root.TryGetProperty("pageSize", out _));
        Assert.True(root.TryGetProperty("totalCount", out _));

        var body = JsonSerializer.Deserialize<PagedEventsResponse>(raw, JsonOptions);
        Assert.NotNull(body);
        Assert.Single(body!.Items);
        Assert.Equal(id, body.Items[0].Id);
        Assert.Equal(42, body.TotalCount);
    }

    [Fact]
    public async Task Get_events_invalid_created_range_returns_400_problem_details()
    {
        factory.CapturingEventListReader.Reset();
        var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/api/events?createdFrom=2026-02-01T00:00:00Z&createdTo=2026-01-01T00:00:00Z");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Empty(factory.CapturingEventListReader.Calls);
    }

    [Fact]
    public async Task Get_events_invalid_type_returns_400_problem_details()
    {
        factory.CapturingEventListReader.Reset();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/events?type=notAnEventType");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(doc.RootElement.TryGetProperty("errors", out var errors));
        Assert.True(errors.TryGetProperty("type", out _));
        Assert.Empty(factory.CapturingEventListReader.Calls);
    }
}
