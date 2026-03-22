using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace EventHub.FunctionApp;

public class HealthFunction(ILogger<HealthFunction> logger)
{
    [Function(nameof(Health))]
    public HttpResponseData Health(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")] HttpRequestData req)
    {
        logger.LogInformation("Health check");
        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        response.WriteString("""{"status":"ok"}""");
        return response;
    }
}
