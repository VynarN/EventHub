using Microsoft.Extensions.Configuration;

namespace EventHub.WebApi.Tests;

internal static class TestConfiguration
{
    internal static IConfiguration Load()
    {
        var testAssemblyDir = Path.GetDirectoryName(typeof(TestConfiguration).Assembly.Location)
            ?? AppContext.BaseDirectory;
        var repoRoot = Path.GetFullPath(Path.Combine(testAssemblyDir, "..", "..", "..", ".."));
        var webApiPath = Path.Combine(repoRoot, "EventHub.WebApi");

        return new ConfigurationBuilder()
            .SetBasePath(webApiPath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
    }
}
