using EventHub.WebApi.Data;
using EventHub.WebApi.Interfaces.Data;
using EventHub.WebApi.Models.EMs;
using EventHub.WebApi.Models.VMs;

namespace EventHub.WebApi.Tests;

public sealed class CapturingEventListReader : IEventListReader
{
    public List<(
        int PageNumber,
        int PageSize,
        string? TypeFilter,
        string? UserIdFilter,
        DateTime? CreatedFromUtc,
        DateTime? CreatedToUtc)> Calls { get; } = new();

    public PagedEventsResponse NextResponse { get; set; } = new()
    {
        Items = new List<Event>(),
        PageNumber = 1,
        PageSize = EventCosmosQueryBuilder.DefaultPageSize,
        TotalCount = 0,
    };

    public Task<PagedEventsResponse> ListAsync(
        int pageNumber,
        int pageSize,
        string? typeFilter,
        string? userIdFilter,
        DateTime? createdFromUtc,
        DateTime? createdToUtc,
        CancellationToken cancellationToken)
    {
        Calls.Add((pageNumber, pageSize, typeFilter, userIdFilter, createdFromUtc, createdToUtc));
        return Task.FromResult(NextResponse);
    }

    public void Reset()
    {
        Calls.Clear();
        NextResponse = new PagedEventsResponse
        {
            Items = new List<Event>(),
            PageNumber = 1,
            PageSize = EventCosmosQueryBuilder.DefaultPageSize,
            TotalCount = 0,
        };
    }
}
