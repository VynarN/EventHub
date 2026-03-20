using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace EventHub.WebApi.Tests;

public class WeatherForecastTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task GetWeatherForecast_ReturnsSuccessAndJsonArray()
    {
        var client = factory.CreateClient();
        var response = await client.GetAsync("/weatherforecast");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("temperatureC", json, StringComparison.OrdinalIgnoreCase);
    }
}
