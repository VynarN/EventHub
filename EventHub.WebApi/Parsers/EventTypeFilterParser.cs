using System.Text.Json;
using System.Text.Json.Serialization;
using EventHub.WebApi.Models;

namespace EventHub.WebApi.Parsers;

public static class EventTypeFilterParser
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    /// <summary>Parses optional query string type filter; returns canonical JSON enum string for Cosmos (e.g. click, pageView).</summary>
    public static bool TryParse(string? typeQueryValue, out string? cosmosTypeValue, out string? errorMessage)
    {
        cosmosTypeValue = null;
        errorMessage = null;
        if (string.IsNullOrWhiteSpace(typeQueryValue))
            return true;

        try
        {
            var json = JsonSerializer.Serialize(typeQueryValue.Trim());
            var parsed = JsonSerializer.Deserialize<EventType>(json, JsonOptions);
            if (!Enum.IsDefined(typeof(EventType), parsed))
            {
                errorMessage = "Invalid type filter.";
                return false;
            }

            var token = JsonSerializer.Serialize(parsed, JsonOptions);
            using var doc = JsonDocument.Parse(token);
            cosmosTypeValue = doc.RootElement.GetString();
            return cosmosTypeValue is not null;
        }
        catch (JsonException)
        {
            errorMessage = "Invalid type filter.";
            return false;
        }
    }
}
