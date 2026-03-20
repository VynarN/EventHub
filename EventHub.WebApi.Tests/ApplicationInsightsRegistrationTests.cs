using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EventHub.WebApi.Tests;

public sealed class ApplicationInsightsRegistrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public ApplicationInsightsRegistrationTests(TestWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public void ApplicationInsights_TelemetryClient_IsRegistered()
    {
        var client = _factory.Services.GetRequiredService<TelemetryClient>();
        Assert.NotNull(client);
    }

    [Fact]
    public void ApplicationInsights_TelemetryConfiguration_IsRegistered()
    {
        var config = _factory.Services.GetRequiredService<TelemetryConfiguration>();
        Assert.NotNull(config);
    }
}
