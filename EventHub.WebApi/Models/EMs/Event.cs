namespace EventHub.WebApi.Models.EMs;

public class Event
{
    public Guid Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public EventType Type { get; set; }

    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
