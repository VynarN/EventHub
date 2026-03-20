using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using EventHub.WebApi.Interfaces.Services;
using EventHub.WebApi.Models;
using EventHub.WebApi.Models.VMs;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EventHub.WebApi.Tests;

public class ExceptionHandlingTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    private static WebApplicationFactory<Program> CreateFactoryWithThrowingPublisher()
    {
        return new TestWebApplicationFactory().WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                foreach (var d in services.Where(d => d.ServiceType == typeof(IEventPublisher)).ToList())
                {
                    services.Remove(d);
                }

                services.AddSingleton<IEventPublisher, ThrowingEventPublisher>();
            });
        });
    }

    [Fact]
    public async Task Unhandled_exception_returns_500_rfc9457_problem_details()
    {
        await using var factory = CreateFactoryWithThrowingPublisher();
        var client = factory.CreateClient();
        var payload = new EventCreationRequest
        {
            UserId = "user-1",
            Type = EventType.Click,
            Description = "x",
        };

        using var response = await client.PostAsJsonAsync("/api/events", payload, JsonOptions);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = doc.RootElement;
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.6.1", root.GetProperty("type").GetString());
        Assert.Equal(500, root.GetProperty("status").GetInt32());
        Assert.False(string.IsNullOrWhiteSpace(root.GetProperty("title").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(root.GetProperty("detail").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(root.GetProperty("instance").GetString()));
        Assert.Equal("/api/events", root.GetProperty("instance").GetString());
    }

    [Fact]
    public async Task Validation_error_includes_title_detail_instance()
    {
        await using var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();
        using var response = await client.PostAsync(
            "/api/events",
            JsonContent.Create(
                new { userId = "", type = "pageView", description = "x" },
                options: JsonOptions));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = doc.RootElement;
        Assert.False(string.IsNullOrWhiteSpace(root.GetProperty("title").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(root.GetProperty("detail").GetString()));
        Assert.Equal("/api/events", root.GetProperty("instance").GetString());
    }
}
