using EventHub.WebApi.Data;
using Xunit;

namespace EventHub.WebApi.Tests;

public class EventCosmosQueryBuilderTests
{
    [Theory]
    [InlineData(0, 0, 1, 20, 0)]
    [InlineData(1, 10, 1, 10, 0)]
    [InlineData(3, 10, 3, 10, 20)]
    [InlineData(2, 200, 2, 100, 100)]
    public void NormalizePagination_clamps_and_computes_offset(
        int pageNumber,
        int pageSize,
        int expectedPage,
        int expectedSize,
        int expectedOffset)
    {
        var (pn, ps, offset) = EventCosmosQueryBuilder.NormalizePagination(pageNumber, pageSize);
        Assert.Equal(expectedPage, pn);
        Assert.Equal(expectedSize, ps);
        Assert.Equal(expectedOffset, offset);
    }

    [Fact]
    public void BuildSelectSql_includes_filters_and_order_offset_limit()
    {
        var sql = EventCosmosQueryBuilder.BuildSelectSql("click", "user-1", null, null);
        Assert.Contains("c.type = @type", sql, StringComparison.Ordinal);
        Assert.Contains("c.userId = @userId", sql, StringComparison.Ordinal);
        Assert.Contains("ORDER BY c.createdAt DESC", sql, StringComparison.Ordinal);
        Assert.Contains("OFFSET @offset", sql, StringComparison.Ordinal);
        Assert.Contains("LIMIT @limit", sql, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildCountSql_matches_filter_shape_without_pagination()
    {
        var sql = EventCosmosQueryBuilder.BuildCountSql("pageView", null, null, null);
        Assert.StartsWith("SELECT VALUE COUNT(1)", sql, StringComparison.Ordinal);
        Assert.Contains("c.type = @type", sql, StringComparison.Ordinal);
        Assert.DoesNotContain("OFFSET", sql, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildSelectSql_without_filters_uses_base_predicate_only()
    {
        var sql = EventCosmosQueryBuilder.BuildSelectSql(null, null, null, null);
        Assert.Contains("IS_DEFINED(c.userId)", sql, StringComparison.Ordinal);
        Assert.DoesNotContain("c.type = @type", sql, StringComparison.Ordinal);
        Assert.DoesNotContain("c.userId = @userId", sql, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildSelectSql_includes_createdAt_range_when_provided()
    {
        var from = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = new DateTime(2026, 1, 31, 23, 59, 59, DateTimeKind.Utc);
        var sql = EventCosmosQueryBuilder.BuildSelectSql(null, null, from, to);
        Assert.Contains("c.createdAt >= @createdFrom", sql, StringComparison.Ordinal);
        Assert.Contains("c.createdAt <= @createdTo", sql, StringComparison.Ordinal);
    }
}
