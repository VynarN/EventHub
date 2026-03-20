using EventHub.WebApi.Interfaces.Data;
using EventHub.WebApi.Models.EMs;
using EventHub.WebApi.Models.VMs;

namespace EventHub.WebApi.Data;

public sealed class NoOpEventListReader : IEventListReader
{
    public Task<PagedEventsResponse> ListAsync(
        int pageNumber,
        int pageSize,
        string? typeFilter,
        string? userIdFilter,
        DateTime? createdFromUtc,
        DateTime? createdToUtc,
        CancellationToken cancellationToken)
    {
        var (pn, ps, _) = EventCosmosQueryBuilder.NormalizePagination(pageNumber, pageSize);
        return Task.FromResult(new PagedEventsResponse
        {
            Items = new List<Event>(),
            PageNumber = pn,
            PageSize = ps,
            TotalCount = 0,
        });
    }
}
