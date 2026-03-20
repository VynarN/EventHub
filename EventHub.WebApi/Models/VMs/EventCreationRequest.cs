using System.ComponentModel.DataAnnotations;

namespace EventHub.WebApi.Models.VMs;

public class EventCreationRequest
{
    [Required]
    [MinLength(1)]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public EventType? Type { get; set; }

    public string Description { get; set; } = string.Empty;
}
