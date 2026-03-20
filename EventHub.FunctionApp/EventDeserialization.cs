using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using EventHub.FunctionApp.Models;

namespace EventHub.FunctionApp;

public static class EventDeserialization
{
    public static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    public static bool TryDeserialize(ReadOnlySpan<byte> utf8Json, [NotNullWhen(true)] out Event? evt, out JsonException? jsonException)
    {
        evt = null;
        jsonException = null;
        try
        {
            var deserialized = JsonSerializer.Deserialize<Event>(utf8Json, SerializerOptions);
            if (deserialized is null)
                return false;
            evt = deserialized;
            return true;
        }
        catch (JsonException ex)
        {
            jsonException = ex;
            return false;
        }
    }

    public static bool TryDeserialize(BinaryData body, [NotNullWhen(true)] out Event? evt, out JsonException? jsonException) =>
        TryDeserialize(body.ToMemory().Span, out evt, out jsonException);

    /// <summary>Canonical JSON enum string for Cosmos (e.g. pageView), aligned with Web API publisher.</summary>
    public static string SerializeEventType(EventType type) =>
        JsonSerializer.Serialize(type, SerializerOptions).Trim('"');
}
