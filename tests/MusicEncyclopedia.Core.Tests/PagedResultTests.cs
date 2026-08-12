using MusicEncyclopedia.Core.DTOs;

namespace MusicEncyclopedia.Core.Tests;

public class PagedResultTests
{
    [Fact]
    public void Create_CalculatesTotalPagesCorrectly()
    {
        // 25 items at page size 10 → 3 pages (ceiling of 2.5)
        var result = PagedResult<int>.Create(
            items: Enumerable.Range(1, 10).ToList(),
            page: 1,
            pageSize: 10,
            totalItems: 25);

        result.TotalPages.Should().Be(3);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalItems.Should().Be(25);
        result.Items.Should().HaveCount(10);
    }

    [Fact]
    public void Create_ExactDivision_YieldsWholeNumberOfPages()
    {
        // 20 items at page size 10 → exactly 2 pages
        var result = PagedResult<int>.Create([], page: 2, pageSize: 10, totalItems: 20);

        result.TotalPages.Should().Be(2);
    }

    [Fact]
    public void Create_NoItems_YieldsZeroPages()
    {
        var result = PagedResult<int>.Create([], page: 1, pageSize: 24, totalItems: 0);

        result.TotalPages.Should().Be(0);
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public void Create_PageSizeLargerThanTotal_YieldsSinglePage()
    {
        var result = PagedResult<int>.Create([], page: 1, pageSize: 100, totalItems: 5);

        result.TotalPages.Should().Be(1);
    }

    [Fact]
    public void Create_PreservesItems()
    {
        var items = new[] { "fa", "en", "ar", "fr" };

        var result = PagedResult<string>.Create(items, page: 1, pageSize: 2, totalItems: 4);

        result.Items.Should().Equal("fa", "en", "ar", "fr");
    }
}
