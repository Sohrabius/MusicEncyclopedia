using System.Net;
using MusicEncyclopedia.Core.Constants;

namespace MusicEncyclopedia.Web.Tests;

public sealed class AccessAndContentIntegrityTests(MusicEncyclopediaWebFactory factory)
    : IClassFixture<MusicEncyclopediaWebFactory>
{
    [Fact]
    public async Task AnonymousAdminRequest_RedirectsToLogin()
    {
        using var client = CreateClientWithoutRedirects();

        var response = await client.GetAsync("/admin");

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location?.PathAndQuery.Should().StartWith("/auth/login");
    }

    [Fact]
    public async Task LoginPost_WithoutAntiForgeryToken_IsRejected()
    {
        using var client = CreateClientWithoutRedirects();

        var response = await client.PostAsync("/auth/login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = "nobody@example.test",
            ["Password"] = "Testing-123!"
        }));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AuthenticatedUser_WithoutPermission_IsDeniedAdminAccess()
    {
        var credentials = await factory.CreateUserAsync();
        using var client = CreateClientWithoutRedirects();
        await client.LoginAsync(credentials.Email, credentials.Password);

        var response = await client.GetAsync("/admin/albums");

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location?.PathAndQuery.Should().StartWith("/auth/access-denied");
    }

    [Fact]
    public async Task AuthenticatedUser_WithAlbumPermission_CanOpenAlbumAdmin()
    {
        var credentials = await factory.CreateUserAsync(PermissionConstants.CanManageAlbums);
        using var client = CreateClientWithoutRedirects();
        await client.LoginAsync(credentials.Email, credentials.Password);

        var response = await client.GetAsync("/admin/albums");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AuthorizedAlbumCreate_WithAntiForgeryToken_PersistsAlbum()
    {
        var credentials = await factory.CreateUserAsync(PermissionConstants.CanManageAlbums);
        var categoryId = await factory.GetFirstAlbumCategoryIdAsync();
        var slug = $"created-by-test-{Guid.NewGuid():N}";
        using var client = CreateClientWithoutRedirects();
        await client.LoginAsync(credentials.Email, credentials.Password);
        var token = await client.GetAntiForgeryTokenAsync("/admin/albums/Create");

        var response = await client.PostAsync("/admin/albums/Create", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Title"] = "Integration test album",
            ["Slug"] = slug,
            ["AlbumCategoryId"] = categoryId.ToString(),
            ["__RequestVerificationToken"] = token
        }));

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        (await factory.AlbumExistsAsync(slug)).Should().BeTrue();
    }

    [Theory]
    [InlineData("REGISTERED")]
    [InlineData("RESTRICTED")]
    public async Task ProtectedLyrics_AreNotExposedToAnonymousUsers(string availabilityCode)
    {
        var secret = $"secret-{Guid.NewGuid():N}";
        var slug = await factory.SeedLyricsTrackAsync(availabilityCode, secret);
        using var client = CreateClientWithoutRedirects();

        var response = await client.GetAsync($"/api/v1/tracks/{slug}/lyrics");
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        body.Should().NotContain(secret);
        response.Headers.CacheControl?.NoStore.Should().BeTrue();
    }

    [Fact]
    public async Task RegisteredLyrics_AreVisibleAfterLogin()
    {
        var secret = $"registered-secret-{Guid.NewGuid():N}";
        var slug = await factory.SeedLyricsTrackAsync("REGISTERED", secret);
        var credentials = await factory.CreateUserAsync();
        using var client = CreateClientWithoutRedirects();
        await client.LoginAsync(credentials.Email, credentials.Password);

        var response = await client.GetAsync($"/api/v1/tracks/{slug}/lyrics");
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().Contain(secret);
    }

    [Fact]
    public async Task RestrictedLyrics_AreVisibleOnlyWithPermission()
    {
        var secret = $"restricted-secret-{Guid.NewGuid():N}";
        var slug = await factory.SeedLyricsTrackAsync("RESTRICTED", secret);
        var credentials = await factory.CreateUserAsync(PermissionConstants.CanViewRestrictedLyrics);
        using var client = CreateClientWithoutRedirects();
        await client.LoginAsync(credentials.Email, credentials.Password);

        var response = await client.GetAsync($"/api/v1/tracks/{slug}/lyrics");
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().Contain(secret);
    }

    [Fact]
    public async Task SoftDeletedAlbum_IsHiddenFromPublicPageAndApi()
    {
        var slug = await factory.SeedDeletedAlbumAsync();
        using var client = CreateClientWithoutRedirects();

        var pageResponse = await client.GetAsync($"/fa/albums/{slug}");
        var apiResponse = await client.GetAsync($"/api/v1/albums/{slug}");

        pageResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        apiResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private HttpClient CreateClientWithoutRedirects() => factory.CreateClient(new()
    {
        BaseAddress = new Uri("https://localhost"),
        AllowAutoRedirect = false,
        HandleCookies = true
    });
}
