using Newtonsoft.Json;

namespace EventHub.Cosmos;

/// <summary>JSON shape for event items in the Events container (matches GET query fields and partition key path /Id).</summary>
public sealed class CosmosEventDocument
{
    [JsonProperty("id")]
    public Guid Id { get; set; }

    /// <summary>Partition key value for path /Id (same logical id as <see cref="Id"/>, string form).</summary>
    [JsonProperty("Id")]
    public string PartitionId { get; set; } = "";

    [JsonProperty("userId")]
    public string UserId { get; set; } = "";

    [JsonProperty("type")]
    public string Type { get; set; } = "";

    [JsonProperty("description")]
    public string Description { get; set; } = "";

    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; }

    public static CosmosEventDocument FromEvent(
        Guid id,
        string userId,
        string typeCanonical,
        string description,
        DateTime createdAt)
    {
        var pid = id.ToString("D");
        var utc = createdAt.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(createdAt, DateTimeKind.Utc)
            : createdAt.ToUniversalTime();
        return new CosmosEventDocument
        {
            Id = id,
            PartitionId = pid,
            UserId = userId,
            Type = typeCanonical,
            Description = description,
            CreatedAt = utc,
        };
    }
}
