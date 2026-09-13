using System.Net;
using System.Text.Json;

namespace MusicEncyclopedia.Web.Tests;

public sealed class PublicAndApiIntegrationTests(MusicEncyclopediaWebFactory factory)
    : IClassFixture<MusicEncyclopediaWebFactory>
{
    [Fact]
    public async Task PrimaryMvcAndApiDetails_RenderSeededDataAndConsistentErrors()
    {
        var catalog = await factory.SeedPublicCatalogAsync();
        using var client = CreateClientWithoutRedirects();

        await AssertPageContainsAsync(client, $"/fa/albums/{catalog.AlbumSlug}", catalog.PersianAlbumTitle);
        await AssertPageContainsAsync(client, $"/fa/albums/{catalog.AlbumSlug}", $"Description for {catalog.Marker}");
        await AssertPageContainsAsync(client, $"/fa/tracks/{catalog.TrackSlug}", catalog.Marker);
        await AssertPageContainsAsync(client, $"/fa/people/{catalog.PersonSlug}", catalog.Marker);
        await AssertPageContainsAsync(client, $"/fa/companies/{catalog.CompanySlug}", catalog.Marker);

        foreach (var (path, property, expected) in new[]
        {
            ($"/api/v1/albums/{catalog.AlbumSlug}", "title", $"{catalog.Marker} album"),
            ($"/api/v1/tracks/{catalog.TrackSlug}", "title", $"{catalog.Marker} track"),
            ($"/api/v1/people/{catalog.PersonSlug}", "fullName", $"{catalog.Marker} person"),
            ($"/api/v1/companies/{catalog.CompanySlug}", "name", $"{catalog.Marker} company")
        })
        {
            using var document = await GetJsonAsync(client, path, HttpStatusCode.OK);
            document.RootElement.GetProperty("success").GetBoolean().Should().BeTrue(path);
            document.RootElement.GetProperty("data").GetProperty(property).GetString().Should().Be(expected);
            document.RootElement.GetProperty("errors").GetArrayLength().Should().Be(0);
        }

        (await client.GetAsync("/fa/albums/does-not-exist")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var missing = await GetJsonAsync(client, "/api/v1/albums/does-not-exist", HttpStatusCode.NotFound);
        missing.RootElement.GetProperty("success").GetBoolean().Should().BeFalse();
        missing.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Null);
        missing.RootElement.GetProperty("message").GetString().Should().Contain("not found");

        var redirectedCulture = await client.GetAsync($"/en/albums/{catalog.AlbumSlug}");
        redirectedCulture.StatusCode.Should().Be(HttpStatusCode.Redirect);
        redirectedCulture.Headers.Location?.OriginalString.Should().Be($"/fa/albums/{catalog.AlbumSlug}");
    }

    [Fact]
    public async Task CatalogFiltersSearchAndPaging_ReturnOnlyMatchingRows()
    {
        var catalog = await factory.SeedPublicCatalogAsync();
        using var client = CreateClientWithoutRedirects();

        foreach (var path in new[]
        {
            $"/api/v1/albums?page=1&pageSize=1&q={catalog.Marker}",
            $"/api/v1/albums?page=1&pageSize=1&genre={catalog.GenreSlug}",
            $"/api/v1/albums?page=1&pageSize=1&mood={catalog.MoodSlug}",
            "/api/v1/albums?page=1&pageSize=1&year=1997",
            $"/api/v1/tracks?page=1&pageSize=1&q={catalog.Marker}",
            $"/api/v1/tracks?page=1&pageSize=1&genre={catalog.GenreSlug}",
            $"/api/v1/tracks?page=1&pageSize=1&mood={catalog.MoodSlug}"
        })
        {
            using var document = await GetJsonAsync(client, path, HttpStatusCode.OK);
            var data = document.RootElement.GetProperty("data");
            data.GetProperty("items").GetArrayLength().Should().Be(1, path);
            data.GetProperty("page").GetInt32().Should().Be(1);
            data.GetProperty("pageSize").GetInt32().Should().Be(1);
            data.GetProperty("totalItems").GetInt32().Should().BeGreaterThanOrEqualTo(1);
            data.GetProperty("totalPages").GetInt32().Should().BeGreaterThanOrEqualTo(1);
        }

        await AssertPageContainsAsync(client, $"/fa/albums?year=1997&q={catalog.Marker}", catalog.PersianAlbumTitle);
        await AssertPageContainsAsync(client, $"/fa/tracks?artist={catalog.PersonSlug}", catalog.Marker);

        using var wrongYear = await GetJsonAsync(client, $"/api/v1/albums?q={catalog.Marker}&year=1996", HttpStatusCode.OK);
        wrongYear.RootElement.GetProperty("data").GetProperty("items").GetArrayLength().Should().Be(0);

        using var search = await GetJsonAsync(
            client,
            $"/api/v1/search?q={catalog.Marker}&type=album&page=1&pageSize=1",
            HttpStatusCode.OK);
        var searchData = search.RootElement.GetProperty("data");
        searchData.GetProperty("items").GetArrayLength().Should().Be(1);
        searchData.GetProperty("totalItems").GetInt32().Should().Be(1);
        searchData.GetProperty("items")[0].GetProperty("entityType").GetString().Should().Be("Album");
        searchData.GetProperty("items")[0].GetProperty("url").GetString().Should().Be($"/albums/{catalog.AlbumSlug}");

        await AssertApiFailureAsync(client, "/api/v1/albums?page=0", HttpStatusCode.BadRequest, "page");
        await AssertApiFailureAsync(client, "/api/v1/albums?pageSize=101", HttpStatusCode.BadRequest, "pageSize");
        await AssertApiFailureAsync(client, "/api/v1/albums?year=10000", HttpStatusCode.BadRequest, "year");
        await AssertApiFailureAsync(client, "/api/v1/search", HttpStatusCode.BadRequest, "q");
        await AssertApiFailureAsync(client, "/api/v1/search?q=test&type=unknown", HttpStatusCode.BadRequest, "type");
        await AssertPageContainsAsync(client, "/fa/search?q=test&type=unknown", "No results");
    }

    [Fact]
    public async Task EveryImplementedPublicMvcFamily_RendersItsSeededEntity()
    {
        var catalog = await factory.SeedPublicCatalogAsync();
        using var client = CreateClientWithoutRedirects();
        var paths = new[]
        {
            "/fa/albums", $"/fa/albums/{catalog.AlbumSlug}",
            "/fa/tracks", $"/fa/tracks/{catalog.TrackSlug}",
            "/fa/people", $"/fa/people/{catalog.PersonSlug}",
            "/fa/artists", "/fa/poets", $"/fa/poets/{catalog.PersonSlug}",
            "/fa/companies", $"/fa/companies/{catalog.CompanySlug}",
            "/fa/poems", $"/fa/poems/{catalog.PoemSlug}",
            $"/fa/sung-versions/{catalog.SungVersionSlug}",
            "/fa/genres", $"/fa/genres/{catalog.GenreSlug}",
            "/fa/moods", $"/fa/moods/{catalog.MoodSlug}",
            "/fa/instruments", $"/fa/instruments/{catalog.InstrumentSlug}",
            "/fa/sources", $"/fa/sources/{catalog.SourceSlug}",
            "/fa/locations", $"/fa/locations/{catalog.LocationSlug}",
            "/fa/publications", $"/fa/publications/{catalog.PublicationSlug}",
            "/fa/sessions", $"/fa/sessions/{catalog.SessionSlug}",
            "/fa/events", $"/fa/events/{catalog.EventSlug}",
            "/fa/awards", $"/fa/awards/{catalog.AwardSlug}",
            "/fa/charts", $"/fa/charts/{catalog.ChartSlug}",
            $"/fa/search?q={catalog.Marker}"
        };

        foreach (var path in paths)
        {
            await AssertPageContainsAsync(client, path, catalog.Marker);
        }
    }

    [Fact]
    public async Task EveryImplementedApiFamily_ReturnsAValidEnvelopeWithSeededData()
    {
        var catalog = await factory.SeedPublicCatalogAsync();
        using var client = CreateClientWithoutRedirects();
        var paths = new[]
        {
            $"/api/v1/albums?q={catalog.Marker}", $"/api/v1/albums/{catalog.AlbumSlug}",
            $"/api/v1/tracks?q={catalog.Marker}", $"/api/v1/tracks/{catalog.TrackSlug}",
            $"/api/v1/people?q={catalog.Marker}", $"/api/v1/people/{catalog.PersonSlug}",
            $"/api/v1/companies?q={catalog.Marker}", $"/api/v1/companies/{catalog.CompanySlug}",
            $"/api/v1/poems?q={catalog.Marker}", $"/api/v1/poems/{catalog.PoemSlug}",
            $"/api/v1/sung-versions/{catalog.SungVersionSlug}",
            $"/api/v1/genres?q={catalog.Marker}", $"/api/v1/genres/{catalog.GenreSlug}",
            $"/api/v1/moods?q={catalog.Marker}", $"/api/v1/moods/{catalog.MoodSlug}",
            $"/api/v1/instruments?q={catalog.Marker}", $"/api/v1/instruments/{catalog.InstrumentSlug}",
            $"/api/v1/sessions?q={catalog.Marker}", $"/api/v1/sessions/{catalog.SessionSlug}",
            $"/api/v1/events?q={catalog.Marker}", $"/api/v1/events/{catalog.EventSlug}",
            $"/api/v1/locations?q={catalog.Marker}", $"/api/v1/locations/{catalog.LocationSlug}",
            $"/api/v1/awards?q={catalog.Marker}", $"/api/v1/awards/{catalog.AwardSlug}",
            $"/api/v1/certifications?q={catalog.Marker}",
            $"/api/v1/charts?q={catalog.Marker}", $"/api/v1/charts/{catalog.ChartSlug}",
            $"/api/v1/search?q={catalog.Marker}&page=1&pageSize=100"
        };

        foreach (var path in paths)
        {
            using var document = await GetJsonAsync(client, path, HttpStatusCode.OK);
            document.RootElement.GetProperty("success").GetBoolean().Should().BeTrue(path);
            document.RootElement.GetProperty("data").GetRawText().Should().Contain(catalog.Marker, path);
            document.RootElement.GetProperty("errors").GetArrayLength().Should().Be(0, path);
        }
    }

    [Fact]
    public async Task AdminDeletion_RemovesAWarmedAlbumFromSearch()
    {
        var catalog = await factory.SeedPublicCatalogAsync();
        var credentials = await factory.CreateUserAsync(
            MusicEncyclopedia.Core.Constants.PermissionConstants.CanManageAlbums,
            MusicEncyclopedia.Core.Constants.PermissionConstants.CanDeleteContent);
        using var publicClient = CreateClientWithoutRedirects();
        using var adminClient = CreateClientWithoutRedirects();
        await adminClient.LoginAsync(credentials.Email, credentials.Password);

        var searchPath = $"/api/v1/search?q={catalog.Marker}&type=album";
        using (var warmSearch = await GetJsonAsync(publicClient, searchPath, HttpStatusCode.OK))
        {
            warmSearch.RootElement.GetProperty("data").GetProperty("totalItems").GetInt32().Should().Be(1);
        }

        var token = await adminClient.GetAntiForgeryTokenAsync($"/admin/albums/{catalog.AlbumId}/Edit");
        var deleted = await adminClient.PostAsync(
            $"/admin/albums/{catalog.AlbumId}/Delete",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token
            }));
        deleted.StatusCode.Should().Be(HttpStatusCode.Redirect);

        using var refreshedSearch = await GetJsonAsync(publicClient, searchPath, HttpStatusCode.OK);
        var data = refreshedSearch.RootElement.GetProperty("data");
        data.GetProperty("totalItems").GetInt32().Should().Be(0);
        data.GetProperty("items").GetArrayLength().Should().Be(0);
    }

    private static async Task AssertPageContainsAsync(HttpClient client, string path, string expected)
    {
        var response = await client.GetAsync(path);
        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, $"{path} returned: {body}");
        WebUtility.HtmlDecode(body).Should().Contain(expected, path);
    }

    private static async Task<JsonDocument> GetJsonAsync(HttpClient client, string path, HttpStatusCode status)
    {
        var response = await client.GetAsync(path);
        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(status, $"{path} returned: {body}");
        return JsonDocument.Parse(body);
    }

    private static async Task AssertApiFailureAsync(
        HttpClient client,
        string path,
        HttpStatusCode status,
        string field)
    {
        using var document = await GetJsonAsync(client, path, status);
        document.RootElement.GetProperty("success").GetBoolean().Should().BeFalse();
        document.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Null);
        document.RootElement.GetProperty("errors")[0].GetProperty("field").GetString().Should().Be(field);
    }

    private HttpClient CreateClientWithoutRedirects() => factory.CreateClient(new()
    {
        BaseAddress = new Uri("https://localhost"),
        AllowAutoRedirect = false,
        HandleCookies = true
    });
}
