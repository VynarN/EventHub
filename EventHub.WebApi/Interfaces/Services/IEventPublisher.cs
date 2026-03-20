using EventHub.WebApi.Models.EMs;

namespace EventHub.WebApi.Interfaces.Services;

public interface IEventPublisher
{
    Task PublishEventCreatedAsync(Event evt, CancellationToken cancellationToken = default);
}
