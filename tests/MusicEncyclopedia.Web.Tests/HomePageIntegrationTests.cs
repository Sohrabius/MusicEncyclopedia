using System.Net;
using System.Text.RegularExpressions;

namespace MusicEncyclopedia.Web.Tests;

public sealed class HomePageIntegrationTests(MusicEncyclopediaWebFactory factory)
    : IClassFixture<MusicEncyclopediaWebFactory>
{
    [Theory]
    [InlineData(0)]
    [InlineData(20)]
    [InlineData(21)]
    [InlineData(100)]
    [InlineData(101)]
    public async Task HomeCatalog_ImplementsBoundaryContract(int albumCount)
    {
        var set = await factory.SeedHomeAlbumsAsync(albumCount);
        using var client = CreateClientWithoutRedirects();
        var homePath = $"/fa?category={set.CategoryCode}";

        var response = await client.GetAsync(homePath);
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        response.StatusCode.Should().Be(HttpStatusCode.OK, html);
        html.Should().Contain("<html lang=\"fa\" dir=\"rtl\">");
        html.Should().MatchRegex("<meta name=\"description\" content=\"[^\"]+\"");
        GetAlbumSlugs(html, set.Marker).Should().Equal(set.Slugs.Take(20));
        Regex.IsMatch(
                html,
                $"href=\"/fa\\?category={Regex.Escape(set.CategoryCode)}\"[^>]*aria-current=\"page\"[^>]*>.*?<span class=\"chip__count\">{albumCount}</span>",
                RegexOptions.Singleline)
            .Should().BeTrue();
        html.Should().NotContain("بیشتر بدانید");

        if (albumCount == 0)
        {
            html.Should().Contain("آلبومی یافت نشد");
            html.Should().NotContain("id=\"home-load-more\"");
            html.Should().NotContain("id=\"home-pagination\"");
            return;
        }

        html.Should().Contain("role=\"status\"");
        html.Should().Contain("aria-current=\"page\"");

        if (albumCount == 20)
        {
            html.Should().NotContain("id=\"home-load-more\"");
        }
        else
        {
            html.Should().Contain("<noscript>");
            html.Should().Contain("id=\"home-load-more\"");
            html.Should().Contain("role=\"alert\"");
            html.Should().Contain($"page=2&category={set.CategoryCode}");
        }

        var collected = GetAlbumSlugs(html, set.Marker).ToList();
        var lastAppendPage = Math.Min(5, (int)Math.Ceiling(albumCount / 20d));
        for (var page = 2; page <= lastAppendPage; page++)
        {
            var partial = await client.GetAsync($"/fa/home/albums?page={page}&category={set.CategoryCode}");
            var partialHtml = WebUtility.HtmlDecode(await partial.Content.ReadAsStringAsync());
            partial.StatusCode.Should().Be(HttpStatusCode.OK, partialHtml);
            collected.AddRange(GetAlbumSlugs(partialHtml, set.Marker));
        }

        collected.Should().Equal(set.Slugs.Take(Math.Min(albumCount, 100)));
        collected.Should().OnlyHaveUniqueItems();

        if (albumCount <= 100)
        {
            html.Should().NotContain("id=\"home-pagination\"");
        }
        else
        {
            html.Should().Contain("id=\"home-pagination\" hidden");
            html.Should().Contain("data-home-tail-page=\"6\"");
            html.Should().NotContain("data-home-tail-page=\"5\"");

            var pageSix = await client.GetAsync($"/fa?page=6&category={set.CategoryCode}");
            var pageSixHtml = WebUtility.HtmlDecode(await pageSix.Content.ReadAsStringAsync());
            pageSix.StatusCode.Should().Be(HttpStatusCode.OK, pageSixHtml);
            GetAlbumSlugs(pageSixHtml, set.Marker).Should().Equal(set.Slugs.Skip(100).Take(20));
            pageSixHtml.Should().Contain("aria-current=\"page\">6</span>");
            pageSixHtml.Should().NotContain("id=\"home-load-more\"");
        }
    }

    [Fact]
    public async Task AlbumAppendEndpoint_RejectsPagesOutsideHybridWindow()
    {
        using var client = CreateClientWithoutRedirects();

        (await client.GetAsync("/fa/home/albums?page=0")).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await client.GetAsync("/fa/home/albums?page=6")).StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private static IReadOnlyList<string> GetAlbumSlugs(string html, string marker) =>
        Regex.Matches(html, $"href=\"/fa/albums/(?<slug>{Regex.Escape(marker)}-\\d{{3}})\"")
            .Select(match => match.Groups["slug"].Value)
            .ToArray();

    private HttpClient CreateClientWithoutRedirects() => factory.CreateClient(new()
    {
        BaseAddress = new Uri("https://localhost"),
        AllowAutoRedirect = false,
        HandleCookies = true
    });
}

public sealed class HomeCacheIntegrationTests(MusicEncyclopediaWebFactory factory)
    : IClassFixture<MusicEncyclopediaWebFactory>
{
    [Fact]
    public async Task AlbumDeletion_RefreshesHomeCategoryCountAndAboutStats()
    {
        var set = await factory.SeedHomeAlbumsAsync(1);
        set.FirstAlbumId.Should().HaveValue();
        var albumId = set.FirstAlbumId!.Value;
        var credentials = await factory.CreateUserAsync(
            MusicEncyclopedia.Core.Constants.PermissionConstants.CanManageAlbums,
            MusicEncyclopedia.Core.Constants.PermissionConstants.CanDeleteContent);
        using var publicClient = CreateClientWithoutRedirects();
        using var adminClient = CreateClientWithoutRedirects();
        await adminClient.LoginAsync(credentials.Email, credentials.Password);

        var path = $"/fa?category={set.CategoryCode}";
        var warmHtml = WebUtility.HtmlDecode(await publicClient.GetStringAsync(path));
        GetCategoryCount(warmHtml, set.CategoryCode).Should().Be(1);
        var warmAboutCount = GetAboutAlbumCount(warmHtml);

        var token = await adminClient.GetAntiForgeryTokenAsync($"/admin/albums/{albumId}/Edit");
        var deleted = await adminClient.PostAsync(
            $"/admin/albums/{albumId}/Delete",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token
            }));
        deleted.StatusCode.Should().Be(HttpStatusCode.Redirect);

        var refreshedHtml = WebUtility.HtmlDecode(await publicClient.GetStringAsync(path));
        GetCategoryCount(refreshedHtml, set.CategoryCode).Should().Be(0);
        GetAboutAlbumCount(refreshedHtml).Should().Be(warmAboutCount - 1);
        refreshedHtml.Should().Contain("آلبومی یافت نشد");
    }

    private static int GetCategoryCount(string html, string categoryCode)
    {
        var match = Regex.Match(
            html,
            $"href=\"/fa\\?category={Regex.Escape(categoryCode)}\"[^>]*>.*?<span class=\"chip__count\">(?<count>\\d+)</span>",
            RegexOptions.Singleline);
        match.Success.Should().BeTrue();
        return int.Parse(match.Groups["count"].Value);
    }

    private static int GetAboutAlbumCount(string html)
    {
        var match = Regex.Match(
            html,
            "home-about__stat-value\">(?<count>\\d+)</dd>",
            RegexOptions.Singleline);
        match.Success.Should().BeTrue();
        return int.Parse(match.Groups["count"].Value);
    }

    private HttpClient CreateClientWithoutRedirects() => factory.CreateClient(new()
    {
        BaseAddress = new Uri("https://localhost"),
        AllowAutoRedirect = false,
        HandleCookies = true
    });
}

public sealed class HomeRandomPoemIntegrationTests(MusicEncyclopediaWebFactory factory)
    : IClassFixture<MusicEncyclopediaWebFactory>
{
    [Fact]
    public async Task RandomPoemEndpoint_UsesFallbacksAndPrefersAlbumLinkedTrack()
    {
        using var client = factory.CreateClient(new()
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });

        (await client.GetAsync("/fa/home/random-poem")).StatusCode.Should().Be(HttpStatusCode.NoContent);

        var plainPoemSlug = await factory.SeedPlainPoemAsync();
        var plainHtml = await client.GetStringAsync("/fa/home/random-poem");
        plainHtml.Should().Contain($"/fa/poems/{plainPoemSlug}");
        plainHtml.Should().NotContain("/fa/tracks/");

        var trackSlug = await factory.SeedLyricsTrackAsync("PUBLIC", "home poem lyrics");
        var trackHtml = await client.GetStringAsync("/fa/home/random-poem");
        trackHtml.Should().Contain($"/fa/tracks/{trackSlug}");
        trackHtml.Should().NotContain("/fa/albums/");

        var catalog = await factory.SeedPublicCatalogAsync();
        var albumLinkedHtml = await client.GetStringAsync("/fa/home/random-poem");
        albumLinkedHtml.Should().Contain($"/fa/poems/{catalog.PoemSlug}");
        albumLinkedHtml.Should().Contain($"/fa/tracks/{catalog.TrackSlug}");
        albumLinkedHtml.Should().Contain($"/fa/albums/{catalog.AlbumSlug}");
    }
}
