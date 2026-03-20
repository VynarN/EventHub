using System.Text.Json;
using System.Text.Json.Serialization;
using EventHub.Cosmos;
using EventHub.WebApi.Interfaces.Data;
using EventHub.WebApi.Models;
using EventHub.WebApi.Models.EMs;
using EventHub.WebApi.Models.VMs;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace EventHub.WebApi.Data;

public sealed class CosmosDbEventListReader(
    CosmosClient cosmosClient,
    IOptions<CosmosDbSettings> cosmosOptions) : IEventListReader
{
    private static readonly JsonSerializerOptions EventTypeParseOptions = new()
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    public async Task<PagedEventsResponse> ListAsync(
        int pageNumber,
        int pageSize,
        string? typeFilter,
        string? userIdFilter,
        DateTime? createdFromUtc,
        DateTime? createdToUtc,
        CancellationToken cancellationToken)
    {
        var settings = cosmosOptions.Value;
        var (pn, ps, offset) = EventCosmosQueryBuilder.NormalizePagination(pageNumber, pageSize);
        var container = cosmosClient.GetContainer(settings.DatabaseName, settings.ContainerName);

        var countSql = EventCosmosQueryBuilder.BuildCountSql(typeFilter, userIdFilter, createdFromUtc, createdToUtc);
        var countQuery = BuildQueryDefinition(
            countSql,
            typeFilter,
            userIdFilter,
            createdFromUtc,
            createdToUtc,
            offset: null,
            limit: null);
        var totalCount = await ExecuteCountAsync(container, countQuery, cancellationToken).ConfigureAwait(false);

        var selectSql = EventCosmosQueryBuilder.BuildSelectSql(typeFilter, userIdFilter, createdFromUtc, createdToUtc);
        var dataQuery = BuildQueryDefinition(selectSql, typeFilter, userIdFilter, createdFromUtc, createdToUtc, offset, ps);
        var items = await ExecuteSelectAsync(container, dataQuery, cancellationToken).ConfigureAwait(false);

        return new PagedEventsResponse
        {
            Items = items.ToList(),
            PageNumber = pn,
            PageSize = ps,
            TotalCount = totalCount,
        };
    }

    private static QueryDefinition BuildQueryDefinition(
        string sql,
        string? typeFilter,
        string? userIdFilter,
        DateTime? createdFromUtc,
        DateTime? createdToUtc,
        int? offset,
        int? limit)
    {
        var q = new QueryDefinition(sql);
        if (typeFilter is not null)
            q.WithParameter("@type", typeFilter);
        if (userIdFilter is not null)
            q.WithParameter("@userId", userIdFilter);
        if (createdFromUtc is not null)
            q.WithParameter("@createdFrom", createdFromUtc.Value);
        if (createdToUtc is not null)
            q.WithParameter("@createdTo", createdToUtc.Value);
        if (offset is not null)
            q.WithParameter("@offset", offset.Value);
        if (limit is not null)
            q.WithParameter("@limit", limit.Value);
        return q;
    }

    private static async Task<int> ExecuteCountAsync(
        Container container,
        QueryDefinition query,
        CancellationToken cancellationToken)
    {
        using var feed = container.GetItemQueryIterator<int>(query, requestOptions: new QueryRequestOptions());
        var page = await feed.ReadNextAsync(cancellationToken).ConfigureAwait(false);
        return page.Resource.FirstOrDefault();
    }

    private static async Task<IReadOnlyList<Event>> ExecuteSelectAsync(
        Container container,
        QueryDefinition query,
        CancellationToken cancellationToken)
    {
        using var feed = container.GetItemQueryIterator<CosmosEventDocument>(query, requestOptions: new QueryRequestOptions());
        var page = await feed.ReadNextAsync(cancellationToken).ConfigureAwait(false);
        var list = new List<Event>(page.Count);
        foreach (var doc in page)
        {
            if (MapToEvent(doc) is { } evt)
                list.Add(evt);
        }

        return list;
    }

    private static bool TryParseStoredType(string typeString, out EventType type)
    {
        type = default;
        try
        {
            var json = JsonSerializer.Serialize(typeString);
            type = JsonSerializer.Deserialize<EventType>(json, EventTypeParseOptions)!;
            return Enum.IsDefined(typeof(EventType), type);
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static Event? MapToEvent(CosmosEventDocument doc)
    {
        if (!TryParseStoredType(doc.Type, out var eventType))
            return null;
        return new Event
        {
            Id = doc.Id,
            UserId = doc.UserId,
            Type = eventType,
            Description = doc.Description,
            CreatedAt = doc.CreatedAt.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(doc.CreatedAt, DateTimeKind.Utc)
                : doc.CreatedAt.ToUniversalTime(),
        };
    }
}
