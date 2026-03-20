using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace EventHub.FunctionApp;

public class HealthFunction(ILogger<HealthFunction> logger)
{
    [Function(nameof(Health))]
    public IActionResult Health(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")] HttpRequest req)
    {
        logger.LogInformation("Health check");
        return new OkObjectResult(new { status = "ok" });
    }
}
