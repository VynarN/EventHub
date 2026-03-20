using System.Text.Json;
using System.Text.Json.Serialization;
using EventHub.WebApi.Models;
using EventHub.WebApi.Models.EMs;
using EventHub.WebApi.Services;
using Xunit;

namespace EventHub.WebApi.Tests;

public class EventPublisherMessageTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    [Fact]
    public void Event_created_message_uses_entity_action_subject_and_full_event_body()
    {
        var evt = new Event
        {
            Id = Guid.Parse("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11"),
            UserId = "user-1",
            Type = EventType.Click,
            Description = "cta",
            CreatedAt = new DateTime(2026, 3, 19, 12, 0, 0, DateTimeKind.Utc),
        };

        var message = EventPublisher.CreateEventCreatedMessage(evt);

        Assert.Equal(EventPublisher.EventCreatedSubject, message.Subject);
        Assert.Equal("application/json", message.ContentType);

        var roundTrip = JsonSerializer.Deserialize<Event>(message.Body, JsonOptions);
        Assert.NotNull(roundTrip);
        Assert.Equal(evt.Id, roundTrip!.Id);
        Assert.Equal(evt.UserId, roundTrip.UserId);
        Assert.Equal(evt.Type, roundTrip.Type);
        Assert.Equal(evt.Description, roundTrip.Description);
        Assert.Equal(evt.CreatedAt, roundTrip.CreatedAt);
    }
}
