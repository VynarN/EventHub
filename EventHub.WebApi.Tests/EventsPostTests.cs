using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using EventHub.WebApi.Models;
using EventHub.WebApi.Models.EMs;
using EventHub.WebApi.Models.VMs;
using EventHub.WebApi.Services;
using Xunit;

namespace EventHub.WebApi.Tests;

public class EventsPostTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    [Fact]
    public async Task Post_events_valid_payload_returns_201_with_event_and_location()
    {
        factory.CapturingPublisher.Reset();
        var client = factory.CreateClient();
        var payload = new EventCreationRequest
        {
            UserId = "user-42",
            Type = EventType.Click,
            Description = "cta",
        };

        var response = await client.PostAsJsonAsync("/api/events", payload, JsonOptions);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var body = await response.Content.ReadFromJsonAsync<Event>(JsonOptions);
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body!.Id);

        var locationPath = response.Headers.Location!.IsAbsoluteUri
            ? response.Headers.Location.AbsolutePath
            : response.Headers.Location.ToString();
        Assert.StartsWith("/api/events/", locationPath, StringComparison.Ordinal);
        Assert.Contains(body.Id.ToString("D"), locationPath, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("user-42", body.UserId);
        Assert.Equal(EventType.Click, body.Type);
        Assert.Equal("cta", body.Description);
        Assert.True(body.CreatedAt <= DateTime.UtcNow.AddMinutes(1));
        Assert.True(body.CreatedAt >= DateTime.UtcNow.AddMinutes(-1));

        var published = factory.CapturingPublisher.PublishedEvents;
        Assert.Single(published);
        var sent = published[0];
        Assert.Equal(body.Id, sent.Id);
        Assert.Equal(body.UserId, sent.UserId);
        Assert.Equal(body.Type, sent.Type);
        Assert.Equal(body.Description, sent.Description);
        Assert.Equal(body.CreatedAt, sent.CreatedAt);

        var message = EventPublisher.CreateEventCreatedMessage(sent);
        Assert.Equal(EventPublisher.EventCreatedSubject, message.Subject);
    }

    [Fact]
    public async Task Post_events_empty_userId_returns_400_problem_details()
    {
        var client = factory.CreateClient();
        using var response = await client.PostAsync(
            "/api/events",
            JsonContent.Create(
                new { userId = "", type = "pageView", description = "x" },
                options: JsonOptions));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = doc.RootElement;
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.1", root.GetProperty("type").GetString());
        Assert.Equal(400, root.GetProperty("status").GetInt32());
        Assert.True(root.TryGetProperty("errors", out var errors));
        Assert.True(errors.TryGetProperty("UserId", out _));
    }

    [Fact]
    public async Task Post_events_missing_type_returns_400_problem_details()
    {
        var client = factory.CreateClient();
        using var response = await client.PostAsync(
            "/api/events",
            JsonContent.Create(
                new { userId = "u1", description = "x" },
                options: JsonOptions));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(doc.RootElement.TryGetProperty("errors", out var errors));
        Assert.True(errors.TryGetProperty("Type", out _));
    }

    [Fact]
    public async Task Post_events_invalid_type_string_returns_400()
    {
        var client = factory.CreateClient();
        using var response = await client.PostAsync(
            "/api/events",
            JsonContent.Create(
                new { userId = "u1", type = "notAnEventType", description = "x" },
                options: JsonOptions));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
