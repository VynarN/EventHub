using EventHub.WebApi.Interfaces.Services;
using EventHub.WebApi.Models.EMs;

namespace EventHub.WebApi.Tests;

/// <summary>Throws on publish to exercise global exception handling in integration tests.</summary>
public sealed class ThrowingEventPublisher : IEventPublisher
{
    public Task PublishEventCreatedAsync(Event evt, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("Simulated publisher failure.");
}
