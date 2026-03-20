using System.Text;
using EventHub.FunctionApp;
using EventHub.FunctionApp.Models;
using Xunit;

namespace EventHub.FunctionApp.Tests;

public class EventDeserializationTests
{
    [Fact]
    public void TryDeserialize_ValidCamelCaseJson_ReturnsEvent()
    {
        var json =
            """{"id":"a1b2c3d4-e5f6-7890-abcd-ef1234567890","userId":"u1","type":"pageView","description":"d","createdAt":"2024-01-15T10:30:00Z"}""";
        var bytes = Encoding.UTF8.GetBytes(json);

        var ok = EventDeserialization.TryDeserialize(bytes, out var evt, out var jsonEx);

        Assert.True(ok);
        Assert.Null(jsonEx);
        Assert.NotNull(evt);
        Assert.Equal(Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"), evt!.Id);
        Assert.Equal("u1", evt.UserId);
        Assert.Equal(EventType.PageView, evt.Type);
        Assert.Equal("d", evt.Description);
        Assert.Equal(DateTimeKind.Utc, evt.CreatedAt.Kind);
    }

    [Fact]
    public void TryDeserialize_InvalidJson_ReturnsFalseAndJsonException()
    {
        var bytes = Encoding.UTF8.GetBytes("{ not json");

        var ok = EventDeserialization.TryDeserialize(bytes, out var evt, out var jsonEx);

        Assert.False(ok);
        Assert.Null(evt);
        Assert.NotNull(jsonEx);
    }

    [Fact]
    public void TryDeserialize_TypeEnumInvalid_ReturnsFalseAndJsonException()
    {
        var json =
            """{"id":"a1b2c3d4-e5f6-7890-abcd-ef1234567890","userId":"u1","type":"notAnEnum","description":"d","createdAt":"2024-01-15T10:30:00Z"}""";
        var bytes = Encoding.UTF8.GetBytes(json);

        var ok = EventDeserialization.TryDeserialize(bytes, out var evt, out var jsonEx);

        Assert.False(ok);
        Assert.Null(evt);
        Assert.NotNull(jsonEx);
    }
}
