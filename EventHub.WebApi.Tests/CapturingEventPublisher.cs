using EventHub.WebApi.Interfaces.Services;
using EventHub.WebApi.Models.EMs;

namespace EventHub.WebApi.Tests;

public sealed class CapturingEventPublisher : IEventPublisher
{
    public List<Event> PublishedEvents { get; } = new();

    public void Reset() => PublishedEvents.Clear();

    public Task PublishEventCreatedAsync(Event evt, CancellationToken cancellationToken = default)
    {
        PublishedEvents.Add(evt);
        return Task.CompletedTask;
    }
}
