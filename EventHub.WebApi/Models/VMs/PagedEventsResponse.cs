using EventHub.WebApi.Models.EMs;

namespace EventHub.WebApi.Models.VMs;

public sealed class PagedEventsResponse
{
    public List<Event> Items { get; init; } = new();

    public int PageNumber { get; init; }

    public int PageSize { get; init; }

    public int TotalCount { get; init; }
}
