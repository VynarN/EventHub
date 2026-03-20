namespace EventHub.WebApi.Data;

/// <summary>Builds parameterized Cosmos SQL for listing events (testable without a live account).</summary>
public static class EventCosmosQueryBuilder
{
    public const int DefaultPageSize = 20;

    public const int MaxPageSize = 100;

    private const string BaseWhere =
        "IS_DEFINED(c.userId) AND IS_DEFINED(c.type) AND IS_DEFINED(c.createdAt)";

    public static (int PageNumber, int PageSize, int Offset) NormalizePagination(int pageNumber, int pageSize)
    {
        var pn = pageNumber < 1 ? 1 : pageNumber;
        var raw = pageSize <= 0 ? DefaultPageSize : pageSize;
        var ps = Math.Min(MaxPageSize, raw);
        var offset = (pn - 1) * ps;
        return (pn, ps, offset);
    }

    public static string BuildCountSql(
        string? typeFilter,
        string? userIdFilter,
        DateTime? createdFromUtc,
        DateTime? createdToUtc)
    {
        var where = BuildWhereClause(typeFilter, userIdFilter, createdFromUtc, createdToUtc);
        return $"SELECT VALUE COUNT(1) FROM c WHERE {where}";
    }

    public static string BuildSelectSql(
        string? typeFilter,
        string? userIdFilter,
        DateTime? createdFromUtc,
        DateTime? createdToUtc) =>
        $"SELECT * FROM c WHERE {BuildWhereClause(typeFilter, userIdFilter, createdFromUtc, createdToUtc)} ORDER BY c.createdAt DESC OFFSET @offset LIMIT @limit";

    private static string BuildWhereClause(
        string? typeFilter,
        string? userIdFilter,
        DateTime? createdFromUtc,
        DateTime? createdToUtc)
    {
        var parts = new List<string> { BaseWhere };
        if (typeFilter is not null)
            parts.Add("c.type = @type");
        if (userIdFilter is not null)
            parts.Add("c.userId = @userId");
        if (createdFromUtc is not null)
            parts.Add("c.createdAt >= @createdFrom");
        if (createdToUtc is not null)
            parts.Add("c.createdAt <= @createdTo");
        return string.Join(" AND ", parts);
    }
}
