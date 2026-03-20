using EventHub.WebApi.Models.VMs;

namespace EventHub.WebApi.Interfaces.Data;

public interface IEventListReader
{
    Task<PagedEventsResponse> ListAsync(
        int pageNumber,
        int pageSize,
        string? typeFilter,
        string? userIdFilter,
        DateTime? createdFromUtc,
        DateTime? createdToUtc,
        CancellationToken cancellationToken);
}
