using EventHub.WebApi.Interfaces.Services;
using EventHub.WebApi.Models.EMs;

namespace EventHub.WebApi.Services;

/// <summary>Used when Service Bus is not configured (e.g. Testing).</summary>
public sealed class NoOpEventPublisher : IEventPublisher
{
    public Task PublishEventCreatedAsync(Event evt, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
