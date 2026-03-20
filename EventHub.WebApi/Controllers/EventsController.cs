using EventHub.WebApi.Data;
using EventHub.WebApi.Interfaces.Data;
using EventHub.WebApi.Interfaces.Services;
using EventHub.WebApi.Models.EMs;
using EventHub.WebApi.Models.VMs;
using EventHub.WebApi.Parsers;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.WebApi.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController(
    ILogger<EventsController> logger,
    IEventPublisher eventPublisher,
    IEventListReader eventListReader) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedEventsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedEventsResponse>> List(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = EventCosmosQueryBuilder.DefaultPageSize,
        [FromQuery] string? type = null,
        [FromQuery] string? userId = null,
        [FromQuery] DateTime? createdFrom = null,
        [FromQuery] DateTime? createdTo = null,
        CancellationToken cancellationToken = default)
    {
        if (!EventTypeFilterParser.TryParse(type, out var typeFilter, out var typeError))
        {
            var message = typeError ?? "Invalid type filter.";
            var details = new ValidationProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                Title = "Invalid query parameter.",
                Status = StatusCodes.Status400BadRequest,
                Detail = message,
                Instance = HttpContext.Request.Path.Value,
            };
            details.Errors.Add("type", new[] { message });
            return BadRequest(details);
        }

        var userIdFilter = string.IsNullOrWhiteSpace(userId) ? null : userId.Trim();
        var createdFromUtc = NormalizeUtcOptional(createdFrom);
        var createdToUtc = NormalizeUtcOptional(createdTo);

        if (createdFromUtc is { } cf && createdToUtc is { } ct && cf > ct)
        {
            const string message = "createdFrom must be less than or equal to createdTo.";
            var details = new ValidationProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                Title = "Invalid query parameter.",
                Status = StatusCodes.Status400BadRequest,
                Detail = message,
                Instance = HttpContext.Request.Path.Value,
            };
            details.Errors.Add("createdFrom", new[] { message });
            details.Errors.Add("createdTo", new[] { message });
            return BadRequest(details);
        }

        var result = await eventListReader
            .ListAsync(
                pageNumber,
                pageSize,
                typeFilter,
                userIdFilter,
                createdFromUtc,
                createdToUtc,
                cancellationToken)
            .ConfigureAwait(false);

        logger.LogInformation(
            "Listed events. PageNumber={PageNumber}, PageSize={PageSize}, TotalCount={TotalCount}",
            pageNumber,
            pageSize,
            result.TotalCount);

        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Event), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Event>> Create([FromBody] EventCreationRequest dto, CancellationToken cancellationToken)
    {
        var created = new Event
        {
            Id = Guid.NewGuid(),
            UserId = dto.UserId,
            Type = dto.Type!.Value,
            Description = dto.Description,
            CreatedAt = DateTime.UtcNow,
        };

        await eventPublisher.PublishEventCreatedAsync(created, cancellationToken).ConfigureAwait(false);

        logger.LogInformation(
            "Event created and publish requested. EventId={EventId}, UserId={UserId}, Type={Type}",
            created.Id,
            created.UserId,
            created.Type);

        return Created($"/api/events/{created.Id}", created);
    }

    private static DateTime? NormalizeUtcOptional(DateTime? value)
    {
        if (value is null)
            return null;
        var dt = value.Value;
        return dt.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(dt, DateTimeKind.Utc)
            : dt.ToUniversalTime();
    }
}
