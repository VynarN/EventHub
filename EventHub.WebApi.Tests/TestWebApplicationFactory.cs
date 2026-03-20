using EventHub.WebApi.Interfaces.Data;
using EventHub.WebApi.Interfaces.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace EventHub.WebApi.Tests;

/// <summary>Uses the Testing environment so Cosmos bootstrap is skipped unless explicitly configured.</summary>
public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureTestServices(services =>
        {
            foreach (var d in services.Where(d => d.ServiceType == typeof(IEventPublisher)).ToList())
            {
                services.Remove(d);
            }

            services.AddSingleton<CapturingEventPublisher>();
            services.AddSingleton<IEventPublisher>(sp => sp.GetRequiredService<CapturingEventPublisher>());

            foreach (var d in services.Where(d => d.ServiceType == typeof(IEventListReader)).ToList())
            {
                services.Remove(d);
            }

            services.AddSingleton<CapturingEventListReader>();
            services.AddSingleton<IEventListReader>(sp => sp.GetRequiredService<CapturingEventListReader>());
        });
    }

    public CapturingEventPublisher CapturingPublisher => Services.GetRequiredService<CapturingEventPublisher>();

    public CapturingEventListReader CapturingEventListReader => Services.GetRequiredService<CapturingEventListReader>();
}
